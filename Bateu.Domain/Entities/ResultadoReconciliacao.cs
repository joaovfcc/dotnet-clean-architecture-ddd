using Bateu.Domain.Enums;
using Bateu.Domain.Common;

namespace Bateu.Domain.Entities;

/// <summary>
/// Representa o resultado de um match entre transações
/// </summary>
public class ResultadoReconciliacao : BaseEntity
{
    public Guid ReconciliacaoId { get; private set; }
    public Guid? TransacaoBancoId { get; private set; }
    public Guid? TransacaoSistemaId { get; private set; }
    public MatchStatus MatchStatus { get; private set; }
    public decimal DivergenciaValor { get; private set; }
    public int DivergenciaDias { get; private set; }
    public string? Observacao { get; private set; }
    public double PontuacaoMatch { get; private set; } // 0 A 100 PARA O APROXIMADO

    //Relacionamentos
    public Reconciliacao Reconciliacao { get; private set; }
    public Transacao? TransacaoBanco { get; private set; }
    public Transacao? TransacaoSistema { get; private set; }

    private ResultadoReconciliacao()
    {
    }

    public static ResultadoReconciliacao CriacaoMatchExato(Guid reconciliacaoId, Transacao transacaoBanco,
        Transacao transacaoSistema)
    {
        return new ResultadoReconciliacao
        {
            ReconciliacaoId = reconciliacaoId,
            TransacaoBancoId = transacaoBanco.Id,
            TransacaoSistemaId = transacaoSistema.Id,
            MatchStatus = MatchStatus.Exato,
            DivergenciaValor = 0,
            DivergenciaDias = 0,
            PontuacaoMatch = 100,
            Observacao = "Match Exato, sem divergências (TIPOS IDÊNTICOS, VALOR E DATA)."
        };
    }

    public static ResultadoReconciliacao CriacaoMatchAproximado(
        Guid reconciliacaoId,
        Transacao transacaoBanco,
        Transacao transacaoSistema,
        string observacao)
    {
        var divergenciaData = Math.Abs((transacaoBanco.DataTransacao - transacaoSistema.DataTransacao).Days);
        var divergenciaValor = transacaoBanco.GetDiscrepancia(transacaoSistema);
        var dataScore = Math.Max(0, 100 - (divergenciaData * 10));
        var valorScore = divergenciaValor == 0 ? 100 : Math.Max(0, 100) - (double)(divergenciaValor * 10);
        var pontuacaoMatch = (dataScore + valorScore) / 2;

        return new ResultadoReconciliacao
        {
            ReconciliacaoId = reconciliacaoId,
            TransacaoBancoId = transacaoBanco.Id,
            TransacaoSistemaId = transacaoSistema.Id,
            MatchStatus = MatchStatus.Aproximado,
            DivergenciaValor = divergenciaValor,
            DivergenciaDias = divergenciaData,
            PontuacaoMatch = pontuacaoMatch,
            Observacao = observacao
        };
    }

    public static ResultadoReconciliacao CriacaoUnmatched(
        Guid reconciliacaoId,
        Transacao transacao,
        string razao)
    {
        var banco = transacao.Fonte == FonteTransacao.Banco;

        return new ResultadoReconciliacao
        {
            ReconciliacaoId = reconciliacaoId,
            TransacaoBancoId = banco ? transacao.Id : null,
            TransacaoSistemaId = banco ? null: transacao.Id,
            MatchStatus = MatchStatus.NaoConciliado,
            DivergenciaValor = transacao.Valor,
            DivergenciaDias = 0,
            PontuacaoMatch = 0,
            Observacao = razao
        };
    }

    public void MarcarComoMatchManual(string observation)
    {
        MatchStatus = MatchStatus.Manual;
        Observacao = observation;
        PontuacaoMatch = 100;
        MarcarComoAtualizado();
    }
}