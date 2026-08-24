using ContaCerta.Application.Dashboard;
using ContaCerta.Application.Dashboard.Queries;
using ContaCerta.Domain.Services;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace ContaCerta.Api.Controllers;

[ApiController]
[Route("api/dashboard")]
public class DashboardController : ControllerBase
{
    private readonly ISender _sender;

    public DashboardController(ISender sender)
    {
        _sender = sender;
    }

    [HttpGet("resumo-mensal")]
    [ProducesResponseType(typeof(ResumoMensalDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<ResumoMensalDto>> ResumoMensal(CancellationToken cancellationToken)
    {
        var resumo = await _sender.Send(new ObterResumoMensalQuery(), cancellationToken);
        return Ok(resumo);
    }

    [HttpGet("projecao-fluxo-caixa")]
    [ProducesResponseType(typeof(ProjecaoFluxoCaixa), StatusCodes.Status200OK)]
    public async Task<ActionResult<ProjecaoFluxoCaixa>> ProjecaoFluxoCaixa(
        [FromQuery] Guid? contaId, CancellationToken cancellationToken)
    {
        var projecao = await _sender.Send(new ObterProjecaoFluxoCaixaQuery(contaId), cancellationToken);
        return Ok(projecao);
    }
}
