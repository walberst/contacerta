using ContaCerta.Domain.Interfaces;
using MediatR;

namespace ContaCerta.Application.Categorias.Queries;

public sealed record ListarCategoriasQuery : IRequest<IReadOnlyList<CategoriaDto>>;

public class ListarCategoriasQueryHandler : IRequestHandler<ListarCategoriasQuery, IReadOnlyList<CategoriaDto>>
{
    private readonly ICategoriaRepository _categoriaRepository;

    public ListarCategoriasQueryHandler(ICategoriaRepository categoriaRepository)
    {
        _categoriaRepository = categoriaRepository;
    }

    public async Task<IReadOnlyList<CategoriaDto>> Handle(ListarCategoriasQuery request, CancellationToken cancellationToken)
    {
        var categorias = await _categoriaRepository.ListarAsync(cancellationToken);

        return categorias
            .Select(c => new CategoriaDto(c.Id, c.Nome, c.Tipo, c.OrcamentoMensal, c.PercentuaisAlerta))
            .ToList();
    }
}
