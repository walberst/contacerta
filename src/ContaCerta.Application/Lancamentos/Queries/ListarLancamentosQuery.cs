using ContaCerta.Application.Common.Models;
using ContaCerta.Domain.Interfaces;
using FluentValidation;
using MediatR;

namespace ContaCerta.Application.Lancamentos.Queries;

public sealed record ListarLancamentosQuery(
    Guid? ContaId,
    Guid? CategoriaId,
    DateOnly? DataInicio,
    DateOnly? DataFim,
    int Pagina = 1,
    int TamanhoPagina = 20) : IRequest<PaginatedResult<LancamentoDto>>;

public class ListarLancamentosQueryValidator : AbstractValidator<ListarLancamentosQuery>
{
    public ListarLancamentosQueryValidator()
    {
        RuleFor(q => q.Pagina).GreaterThanOrEqualTo(1);
        RuleFor(q => q.TamanhoPagina).InclusiveBetween(1, 100);
    }
}

public class ListarLancamentosQueryHandler : IRequestHandler<ListarLancamentosQuery, PaginatedResult<LancamentoDto>>
{
    private readonly ILancamentoRepository _lancamentoRepository;

    public ListarLancamentosQueryHandler(ILancamentoRepository lancamentoRepository)
    {
        _lancamentoRepository = lancamentoRepository;
    }

    public async Task<PaginatedResult<LancamentoDto>> Handle(ListarLancamentosQuery request, CancellationToken cancellationToken)
    {
        var filtro = new FiltroLancamentos(
            request.ContaId, request.CategoriaId, request.DataInicio, request.DataFim, request.Pagina, request.TamanhoPagina);

        var pagina = await _lancamentoRepository.ListarPaginadoAsync(filtro, cancellationToken);

        var itens = pagina.Itens
            .Select(l => new LancamentoDto(
                l.Id,
                l.ContaId,
                l.Conta?.Nome ?? string.Empty,
                l.CategoriaId,
                l.Categoria?.Nome ?? string.Empty,
                l.Tipo,
                l.Valor,
                l.Data,
                l.Descricao,
                l.Recorrencia))
            .ToList();

        return new PaginatedResult<LancamentoDto>(itens, pagina.TotalDeItens, pagina.Pagina, pagina.TamanhoPagina);
    }
}
