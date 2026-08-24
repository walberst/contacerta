using ContaCerta.Api.Contracts;
using ContaCerta.Application.Contas;
using ContaCerta.Application.Contas.Commands;
using ContaCerta.Application.Contas.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace ContaCerta.Api.Controllers;

[ApiController]
[Route("api/contas")]
public class ContasController : ControllerBase
{
    private readonly ISender _sender;

    public ContasController(ISender sender)
    {
        _sender = sender;
    }

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<ContaDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<ContaDto>>> Listar(CancellationToken cancellationToken)
    {
        var contas = await _sender.Send(new ListarContasQuery(), cancellationToken);
        return Ok(contas);
    }

    [HttpPost]
    [ProducesResponseType(typeof(ContaDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ContaDto>> Criar(CriarContaRequest request, CancellationToken cancellationToken)
    {
        var conta = await _sender.Send(new CriarContaCommand(request.Nome, request.Tipo, request.SaldoInicial), cancellationToken);
        return CreatedAtAction(nameof(Listar), conta);
    }
}
