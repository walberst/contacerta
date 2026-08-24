using ContaCerta.Domain.Common;
using ContaCerta.Domain.Enums;
using ContaCerta.Domain.Events;
using ContaCerta.Domain.Exceptions;

namespace ContaCerta.Domain.Entities;

/// <summary>
/// Receita ou despesa vinculada a uma conta e a uma categoria. Um lancamento marcado
/// como recorrente mensal se repete indefinidamente a partir da data original, no
/// mesmo dia do mes, e e a base da projecao de fluxo de caixa.
/// </summary>
public class Lancamento : BaseEntity
{
    public Guid ContaId { get; private set; }
    public Guid CategoriaId { get; private set; }
    public TipoLancamento Tipo { get; private set; }
    public decimal Valor { get; private set; }
    public DateOnly Data { get; private set; }
    public string Descricao { get; private set; } = string.Empty;
    public Recorrencia Recorrencia { get; private set; }
    public DateTime DataCriacao { get; private set; }

    public Conta? Conta { get; private set; }
    public Categoria? Categoria { get; private set; }

    public bool EhRecorrente => Recorrencia != Recorrencia.Nenhuma;

    private Lancamento()
    {
    }

    public static Lancamento Criar(
        Guid contaId,
        Guid categoriaId,
        TipoLancamento tipo,
        decimal valor,
        DateOnly data,
        string descricao,
        Recorrencia recorrencia = Recorrencia.Nenhuma)
    {
        if (valor <= 0)
        {
            throw new ValorInvalidoException("Valor do lancamento deve ser maior que zero.");
        }

        if (string.IsNullOrWhiteSpace(descricao))
        {
            throw new ValorInvalidoException("Descricao do lancamento e obrigatoria.");
        }

        return new Lancamento
        {
            ContaId = contaId,
            CategoriaId = categoriaId,
            Tipo = tipo,
            Valor = valor,
            Data = data,
            Descricao = descricao.Trim(),
            Recorrencia = recorrencia,
            DataCriacao = DateTime.UtcNow
        };
    }

    public void RegistrarAlertaOrcamento(LimiteOrcamentoAtingidoEvent evento) => AddDomainEvent(evento);

    /// <summary>
    /// Datas em que esse lancamento recorrente mensal ocorre dentro da janela
    /// (referencia, fim]. Usado pela projecao de fluxo de caixa; um lancamento nao
    /// recorrente nunca gera ocorrencias futuras alem da propria data original.
    /// </summary>
    public IEnumerable<DateOnly> OcorrenciasEntre(DateOnly referenciaExclusiva, DateOnly fimInclusivo)
    {
        if (Recorrencia != Recorrencia.Mensal)
        {
            yield break;
        }

        var ocorrencia = Data;
        while (ocorrencia <= referenciaExclusiva)
        {
            ocorrencia = ocorrencia.AddMonths(1);
        }

        while (ocorrencia <= fimInclusivo)
        {
            yield return ocorrencia;
            ocorrencia = ocorrencia.AddMonths(1);
        }
    }
}
