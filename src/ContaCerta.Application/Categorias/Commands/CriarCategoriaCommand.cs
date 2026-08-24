using ContaCerta.Domain.Entities;
using ContaCerta.Domain.Enums;
using ContaCerta.Domain.Interfaces;
using FluentValidation;
using MediatR;

namespace ContaCerta.Application.Categorias.Commands;

public sealed record CriarCategoriaCommand(
    string Nome,
    TipoLancamento Tipo,
    decimal OrcamentoMensal,
    IReadOnlyCollection<int>? PercentuaisAlerta) : IRequest<CategoriaDto>;

public class CriarCategoriaCommandValidator : AbstractValidator<CriarCategoriaCommand>
{
    public CriarCategoriaCommandValidator()
    {
        RuleFor(c => c.Nome).NotEmpty().MaximumLength(80);
        RuleFor(c => c.Tipo).IsInEnum();
        RuleFor(c => c.OrcamentoMensal).GreaterThanOrEqualTo(0);
        RuleForEach(c => c.PercentuaisAlerta).InclusiveBetween(1, 300);
    }
}

public class CriarCategoriaCommandHandler : IRequestHandler<CriarCategoriaCommand, CategoriaDto>
{
    private readonly ICategoriaRepository _categoriaRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CriarCategoriaCommandHandler(ICategoriaRepository categoriaRepository, IUnitOfWork unitOfWork)
    {
        _categoriaRepository = categoriaRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<CategoriaDto> Handle(CriarCategoriaCommand request, CancellationToken cancellationToken)
    {
        var categoria = Categoria.Criar(request.Nome, request.Tipo, request.OrcamentoMensal, request.PercentuaisAlerta);
        _categoriaRepository.Adicionar(categoria);

        await _unitOfWork.SalvarAlteracoesAsync(cancellationToken);

        return new CategoriaDto(categoria.Id, categoria.Nome, categoria.Tipo, categoria.OrcamentoMensal, categoria.PercentuaisAlerta);
    }
}
