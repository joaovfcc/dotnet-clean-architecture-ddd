using Bateu.Domain.Common;
using Bateu.Domain.Enums;

namespace Bateu.Domain.Entities;

/// <summary>
/// Representa um processo de conciliação entre extratos bancários e sistema contábil
/// </summary>
public class Reconciliacao : BaseEntity
{
    public string UserId { get; private set; }
    public StatusReconciliacao Status { get; private set; }
    public DateTime? ProcessamentoIniciado { get; private set; }
    public DateTime? ProcessamentoFinalizado { get; private set; }
    public string? ErrorMessage { get; private set; }

    //Estatisticas
    public int TotalTransacoesBanco { get; private set; }
    public int TotalTransacoesSistema { get; private set; }
    public int MatchesExatos { get; private set; }
    public int MatchesAproximados { get; private set; }
    public int TransacoesBancariasNaoConciliadas { get; private set; }
    public int TransacoesSistemaNaoConciliadas { get; private set; }
    public decimal ValorTotalDivergencias { get; private set; }

    //Relacionamentos
    public ICollection<Transacao> Transacoes { get; private set; }
    public ICollection<ResultadoReconciliacao> ResultadosConciliacao { get; private set; }

    private Reconciliacao()
    {
        Transacoes = new List<Transacao>();
        ResultadosConciliacao = new List<ResultadoReconciliacao>();
    }

    public Reconciliacao(string userId) : this()
    {
        UserId = userId;
        Status = StatusReconciliacao.Pendente;
    }

    public void IniciarProcessamento()
    {
        Status = StatusReconciliacao.EmAndamento;
        ProcessamentoIniciado = DateTime.UtcNow;
        MarcarComoAtualizado();
    }

    public void FinalizarProcessamento(
        int matchesExatos,
        int matchesAproximados,
        int transacoesBancariasNaoConciliadas,
        int transacoesSistemaNaoConciliadas,
        decimal valorTotalDivergencias)
    {
        Status = TransacoesBancariasNaoConciliadas > 0 || TransacoesSistemaNaoConciliadas > 0
            ? StatusReconciliacao.ParciamenteConcluida
            : StatusReconciliacao.Concluida;

        ProcessamentoFinalizado = DateTime.UtcNow;
        MatchesExatos = matchesExatos;
        MatchesAproximados = matchesAproximados;
        TransacoesBancariasNaoConciliadas = transacoesBancariasNaoConciliadas;
        TransacoesSistemaNaoConciliadas = transacoesSistemaNaoConciliadas;
        ValorTotalDivergencias = valorTotalDivergencias;
        MarcarComoAtualizado();
    }

    public void MarcarComoFalhada(string errorMessage)
    {
        Status = StatusReconciliacao.Falhada;
        ErrorMessage = errorMessage;
        ProcessamentoFinalizado = DateTime.UtcNow;
        MarcarComoAtualizado();
    }

    public void AtualizarEstatisticas(int contagemBanco, int contagemSistema)
    {
        TotalTransacoesBanco = contagemBanco;
        TotalTransacoesSistema = contagemSistema;
        MarcarComoAtualizado();
    }
}