using ContaCerta.Application.Common.Interfaces;
using ContaCerta.Domain.Enums;
using ContaCerta.Domain.Interfaces;
using MediatR;

namespace ContaCerta.Application.Categorias.Queries;

/// <summary>
/// Alimenta a tela de orcamento por categoria: para cada categoria de despesa,
/// mostra quanto ja foi gasto no mes corrente frente ao orcamento definido.
/// </summary>
public sealed record ListarOrcamentoCategoriasQuery : IRequest<IReadOnlyList<CategoriaOrcamentoDto>>;

public class ListarOrcamentoCategoriasQueryHandler
    : IRequestHandler<ListarOrcamentoCategoriasQuery, IReadOnlyList<CategoriaOrcamentoDto>>
{
    private readonly ICategoriaRepository _categoriaRepository;
    private readonly IDateTimeProvider _dateTimeProvider;

    public ListarOrcamentoCategoriasQueryHandler(
        ICategoriaRepository categoriaRepository,
        IDateTimeProvider dateTimeProvider)
    {
        _categoriaRepository = categoriaRepository;
        _dateTimeProvider = dateTimeProvider;
    }

    public async Task<IReadOnlyList<CategoriaOrcamentoDto>> Handle(
        ListarOrcamentoCategoriasQuery request, CancellationToken cancellationToken)
    {
        var hoje = _dateTimeProvider.Hoje;
        var categorias = await _categoriaRepository.ListarAsync(cancellationToken);
        var categoriasDeDespesa = categorias.Where(c => c.Tipo == TipoLancamento.Despesa).ToList();

        var resultado = new List<CategoriaOrcamentoDto>(categoriasDeDespesa.Count);
        foreach (var categoria in categoriasDeDespesa)
        {
            var totalGasto = await _categoriaRepository.ObterTotalGastoNoMesAsync(
                categoria.Id, hoje.Year, hoje.Month, cancellationToken);

            var percentualUtilizado = categoria.OrcamentoMensal > 0
                ? Math.Round(totalGasto / categoria.OrcamentoMensal * 100m, 1)
                : 0m;

            resultado.Add(new CategoriaOrcamentoDto(
                categoria.Id,
                categoria.Nome,
                categoria.OrcamentoMensal,
                totalGasto,
                percentualUtilizado,
                categoria.OrcamentoMensal > 0 && totalGasto >= categoria.OrcamentoMensal));
        }

        return resultado;
    }
}
