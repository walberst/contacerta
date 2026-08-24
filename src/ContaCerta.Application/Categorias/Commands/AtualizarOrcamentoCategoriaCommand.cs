using ContaCerta.Application.Common.Exceptions;
using ContaCerta.Domain.Interfaces;
using FluentValidation;
using MediatR;

namespace ContaCerta.Application.Categorias.Commands;

public sealed record AtualizarOrcamentoCategoriaCommand(Guid CategoriaId, decimal NovoOrcamentoMensal) : IRequest<Unit>;

public class AtualizarOrcamentoCategoriaCommandValidator : AbstractValidator<AtualizarOrcamentoCategoriaCommand>
{
    public AtualizarOrcamentoCategoriaCommandValidator()
    {
        RuleFor(c => c.NovoOrcamentoMensal).GreaterThanOrEqualTo(0);
    }
}

public class AtualizarOrcamentoCategoriaCommandHandler : IRequestHandler<AtualizarOrcamentoCategoriaCommand, Unit>
{
    private readonly ICategoriaRepository _categoriaRepository;
    private readonly IUnitOfWork _unitOfWork;

    public AtualizarOrcamentoCategoriaCommandHandler(ICategoriaRepository categoriaRepository, IUnitOfWork unitOfWork)
    {
        _categoriaRepository = categoriaRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Unit> Handle(AtualizarOrcamentoCategoriaCommand request, CancellationToken cancellationToken)
    {
        var categoria = await _categoriaRepository.ObterPorIdAsync(request.CategoriaId, cancellationToken)
            ?? throw new NotFoundException("Categoria", request.CategoriaId);

        categoria.AtualizarOrcamento(request.NovoOrcamentoMensal);

        await _unitOfWork.SalvarAlteracoesAsync(cancellationToken);

        return Unit.Value;
    }
}
