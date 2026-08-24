using ContaCerta.Api.Contracts;
using ContaCerta.Application.Categorias;
using ContaCerta.Application.Categorias.Commands;
using ContaCerta.Application.Categorias.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace ContaCerta.Api.Controllers;

[ApiController]
[Route("api/categorias")]
public class CategoriasController : ControllerBase
{
    private readonly ISender _sender;

    public CategoriasController(ISender sender)
    {
        _sender = sender;
    }

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<CategoriaDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<CategoriaDto>>> Listar(CancellationToken cancellationToken)
    {
        var categorias = await _sender.Send(new ListarCategoriasQuery(), cancellationToken);
        return Ok(categorias);
    }

    [HttpGet("orcamento")]
    [ProducesResponseType(typeof(IReadOnlyList<CategoriaOrcamentoDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<CategoriaOrcamentoDto>>> ObterOrcamento(CancellationToken cancellationToken)
    {
        var orcamentos = await _sender.Send(new ListarOrcamentoCategoriasQuery(), cancellationToken);
        return Ok(orcamentos);
    }

    [HttpPost]
    [ProducesResponseType(typeof(CategoriaDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<CategoriaDto>> Criar(CriarCategoriaRequest request, CancellationToken cancellationToken)
    {
        var categoria = await _sender.Send(
            new CriarCategoriaCommand(request.Nome, request.Tipo, request.OrcamentoMensal, request.PercentuaisAlerta),
            cancellationToken);

        return CreatedAtAction(nameof(Listar), categoria);
    }

    [HttpPut("{id:guid}/orcamento")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> AtualizarOrcamento(Guid id, AtualizarOrcamentoRequest request, CancellationToken cancellationToken)
    {
        await _sender.Send(new AtualizarOrcamentoCategoriaCommand(id, request.NovoOrcamentoMensal), cancellationToken);
        return NoContent();
    }
}
