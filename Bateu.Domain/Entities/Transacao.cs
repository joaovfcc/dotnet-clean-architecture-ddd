using System.Globalization;
using Bateu.Domain.Common;
using Bateu.Domain.Enums;

namespace Bateu.Domain.Entities;

/// <summary>
/// Representa uma transação financeira (do banco ou do sistema)
/// </summary>
public class Transacao : BaseEntity
{
    public Guid ReconciliacaoId { get; private set; }
    public FonteTransacao Fonte { get; private set; }
    public TipoTransacao Tipo { get; private set; }
    public DateTime DataTransacao { get; private set; }
    public decimal Valor { get; private set; }
    public string Descricao { get; private set; }
    public string? Referencia { get; private set; }
    public string? Categoria { get; private set; }

//Hash para o Fast Pass Matching
    public string MatchHash { get; private set; }

//Navegação
    public Reconciliacao Reconciliacao { get; private set; } = null !;
    public ResultadoReconciliacao? ResultadoConciliacao { get; private set; }
    
    private Transacao()
    {
    }
    
    public Transacao(
        Guid reconciliacaoId,
        FonteTransacao fonte,
        TipoTransacao tipo,
        DateTime dataTransacao,
        decimal valor,
        string descricao,
        string? referencia = null,
        string? categoria = null)
    {
        ReconciliacaoId = reconciliacaoId;
        Fonte = fonte;
        Tipo = tipo;
        DataTransacao = dataTransacao;
        Valor = valor;
        Descricao = descricao;
        Referencia = referencia;
        Categoria = categoria;
        
        //Para gerar o hash de match rápido
        MatchHash = GerarMatchHash();
    }

    private string GerarMatchHash()
    {
        return string.Create(CultureInfo.InvariantCulture, 
            $"{DataTransacao:yyyyMMdd}_{Valor:F2}_{Tipo}");
    }

    public bool MatchExato(Transacao outro)
    {
        return MatchHash == outro.MatchHash;
    }

    public bool MatchAproximado(Transacao outro, int toleranciaDia, decimal valorTolerancia)
    {
        if (Tipo != outro.Tipo)
            return false;
        
        //verificacao janela temporal
        var diferencaDias = Math.Abs((DataTransacao - outro.DataTransacao).Days);
        if (diferencaDias > toleranciaDia)
            return false;
        
        return true;
    }

    public decimal GetDiscrepancia(Transacao outro)
    {
        return Math.Abs(Valor - outro.Valor);
    }
}