using ContaCerta.Api.Contracts;
using ContaCerta.Application.Common.Models;
using ContaCerta.Application.Lancamentos;
using ContaCerta.Application.Lancamentos.Commands;
using ContaCerta.Application.Lancamentos.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace ContaCerta.Api.Controllers;

[ApiController]
[Route("api/lancamentos")]
public class LancamentosController : ControllerBase
{
    private readonly ISender _sender;

    public LancamentosController(ISender sender)
    {
        _sender = sender;
    }

    [HttpGet]
    [ProducesResponseType(typeof(PaginatedResult<LancamentoDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PaginatedResult<LancamentoDto>>> Listar(
        [FromQuery] Guid? contaId,
        [FromQuery] Guid? categoriaId,
        [FromQuery] DateOnly? dataInicio,
        [FromQuery] DateOnly? dataFim,
        [FromQuery] int pagina = 1,
        [FromQuery] int tamanhoPagina = 20,
        CancellationToken cancellationToken = default)
    {
        var query = new ListarLancamentosQuery(contaId, categoriaId, dataInicio, dataFim, pagina, tamanhoPagina);
        var resultado = await _sender.Send(query, cancellationToken);
        return Ok(resultado);
    }

    [HttpPost]
    [ProducesResponseType(typeof(LancamentoDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<ActionResult<LancamentoDto>> Criar(CriarLancamentoRequest request, CancellationToken cancellationToken)
    {
        var comando = new CriarLancamentoCommand(
            request.ContaId, request.CategoriaId, request.Tipo, request.Valor, request.Data, request.Descricao, request.Recorrencia);

        var lancamento = await _sender.Send(comando, cancellationToken);
        return CreatedAtAction(nameof(Listar), lancamento);
    }
}
