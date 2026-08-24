using ContaCerta.Domain.Events;
using MediatR;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

namespace ContaCerta.Infrastructure.Realtime;

/// <summary>
/// Ponte entre o evento de dominio e o mundo real time: quando uma despesa cruza um
/// percentual de orcamento, empurra o alerta para todo mundo conectado no hub. Sem
/// filtro por usuario porque o escopo do projeto e single tenant.
/// </summary>
public class LimiteOrcamentoAtingidoEventHandler : INotificationHandler<LimiteOrcamentoAtingidoEvent>
{
    private readonly IHubContext<OrcamentoHub> _hubContext;
    private readonly ILogger<LimiteOrcamentoAtingidoEventHandler> _logger;

    public LimiteOrcamentoAtingidoEventHandler(
        IHubContext<OrcamentoHub> hubContext,
        ILogger<LimiteOrcamentoAtingidoEventHandler> logger)
    {
        _hubContext = hubContext;
        _logger = logger;
    }

    public async Task Handle(LimiteOrcamentoAtingidoEvent notification, CancellationToken cancellationToken)
    {
        var payload = new AlertaOrcamentoPayload(
            notification.CategoriaId,
            notification.NomeCategoria,
            notification.ContaId,
            notification.PercentualLimite,
            notification.TotalGastoNoMes,
            notification.OrcamentoMensal,
            notification.Referencia);

        _logger.LogWarning(
            "Categoria {NomeCategoria} cruzou {PercentualLimite}% do orcamento mensal ({TotalGasto}/{Orcamento})",
            notification.NomeCategoria, notification.PercentualLimite, notification.TotalGastoNoMes, notification.OrcamentoMensal);

        await _hubContext.Clients.All.SendAsync("AlertaOrcamento", payload, cancellationToken);
    }
}
