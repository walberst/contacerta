using ContaCerta.Domain.Entities;
using ContaCerta.Domain.Enums;
using ContaCerta.Domain.Events;

namespace ContaCerta.Domain.Services;

/// <summary>
/// Traduz "total gasto antes/depois de um lancamento" em eventos de alerta de
/// orcamento. Fica fora da entidade Lancamento porque depende do total agregado dos
/// outros lancamentos do mes, algo que a entidade isolada nao tem como saber.
/// </summary>
public static class AvaliadorDeOrcamento
{
    public static IReadOnlyCollection<LimiteOrcamentoAtingidoEvent> Avaliar(
        Categoria categoria,
        Lancamento lancamento,
        decimal totalGastoAntesDoLancamento)
    {
        if (lancamento.Tipo != TipoLancamento.Despesa)
        {
            return Array.Empty<LimiteOrcamentoAtingidoEvent>();
        }

        var totalGastoDepois = totalGastoAntesDoLancamento + lancamento.Valor;
        var limitesCruzados = categoria.ObterLimitesCruzados(totalGastoAntesDoLancamento, totalGastoDepois);

        if (limitesCruzados.Count == 0)
        {
            return Array.Empty<LimiteOrcamentoAtingidoEvent>();
        }

        return limitesCruzados
            .Select(percentual => new LimiteOrcamentoAtingidoEvent(
                categoria.Id,
                categoria.Nome,
                lancamento.ContaId,
                percentual,
                totalGastoDepois,
                categoria.OrcamentoMensal,
                lancamento.Data))
            .ToList();
    }
}
