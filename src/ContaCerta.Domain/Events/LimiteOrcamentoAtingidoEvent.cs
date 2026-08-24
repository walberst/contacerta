using MediatR;

namespace ContaCerta.Domain.Events;

/// <summary>
/// Disparado quando um lancamento de despesa faz o total gasto da categoria no mes
/// cruzar um dos percentuais de alerta configurados (ex: 80%, 100%). O handler dessa
/// notificacao, na camada de Infrastructure, e quem empurra o alerta via SignalR.
/// </summary>
public sealed record LimiteOrcamentoAtingidoEvent(
    Guid CategoriaId,
    string NomeCategoria,
    Guid ContaId,
    int PercentualLimite,
    decimal TotalGastoNoMes,
    decimal OrcamentoMensal,
    DateOnly Referencia) : INotification;
