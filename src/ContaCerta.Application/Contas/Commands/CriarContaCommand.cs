using ContaCerta.Application.Common.Exceptions;
using ContaCerta.Domain.Entities;
using ContaCerta.Domain.Enums;
using ContaCerta.Domain.Interfaces;
using FluentValidation;
using MediatR;

namespace ContaCerta.Application.Contas.Commands;

public sealed record CriarContaCommand(string Nome, TipoConta Tipo, decimal SaldoInicial) : IRequest<ContaDto>;

public class CriarContaCommandValidator : AbstractValidator<CriarContaCommand>
{
    public CriarContaCommandValidator()
    {
        RuleFor(c => c.Nome).NotEmpty().MaximumLength(120);
        RuleFor(c => c.Tipo).IsInEnum();
    }
}

public class CriarContaCommandHandler : IRequestHandler<CriarContaCommand, ContaDto>
{
    private readonly IContaRepository _contaRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CriarContaCommandHandler(IContaRepository contaRepository, IUnitOfWork unitOfWork)
    {
        _contaRepository = contaRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<ContaDto> Handle(CriarContaCommand request, CancellationToken cancellationToken)
    {
        var contasExistentes = await _contaRepository.ListarAsync(cancellationToken);
        var jaExiste = contasExistentes.Any(c =>
            string.Equals(c.Nome, request.Nome.Trim(), StringComparison.OrdinalIgnoreCase) && c.Tipo == request.Tipo);

        if (jaExiste)
        {
            throw new ConflictException($"Ja existe uma conta '{request.Nome}' do tipo {request.Tipo}.");
        }

        var conta = Conta.Criar(request.Nome, request.Tipo, request.SaldoInicial);
        _contaRepository.Adicionar(conta);

        await _unitOfWork.SalvarAlteracoesAsync(cancellationToken);

        return new ContaDto(conta.Id, conta.Nome, conta.Tipo, conta.SaldoInicial, conta.SaldoInicial);
    }
}
