using ContaCerta.Application.Common.Exceptions;
using ContaCerta.Domain.Entities;
using ContaCerta.Domain.Enums;
using ContaCerta.Domain.Interfaces;
using ContaCerta.Domain.Services;
using FluentValidation;
using MediatR;

namespace ContaCerta.Application.Lancamentos.Commands;

public sealed record CriarLancamentoCommand(
    Guid ContaId,
    Guid CategoriaId,
    TipoLancamento Tipo,
    decimal Valor,
    DateOnly Data,
    string Descricao,
    Recorrencia Recorrencia) : IRequest<LancamentoDto>;

public class CriarLancamentoCommandValidator : AbstractValidator<CriarLancamentoCommand>
{
    public CriarLancamentoCommandValidator()
    {
        RuleFor(c => c.ContaId).NotEmpty();
        RuleFor(c => c.CategoriaId).NotEmpty();
        RuleFor(c => c.Tipo).IsInEnum();
        RuleFor(c => c.Valor).GreaterThan(0);
        RuleFor(c => c.Descricao).NotEmpty().MaximumLength(200);
        RuleFor(c => c.Data).NotEqual(default(DateOnly));
        RuleFor(c => c.Recorrencia).IsInEnum();
    }
}

public class CriarLancamentoCommandHandler : IRequestHandler<CriarLancamentoCommand, LancamentoDto>
{
    private readonly IContaRepository _contaRepository;
    private readonly ICategoriaRepository _categoriaRepository;
    private readonly ILancamentoRepository _lancamentoRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CriarLancamentoCommandHandler(
        IContaRepository contaRepository,
        ICategoriaRepository categoriaRepository,
        ILancamentoRepository lancamentoRepository,
        IUnitOfWork unitOfWork)
    {
        _contaRepository = contaRepository;
        _categoriaRepository = categoriaRepository;
        _lancamentoRepository = lancamentoRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<LancamentoDto> Handle(CriarLancamentoCommand request, CancellationToken cancellationToken)
    {
        var conta = await _contaRepository.ObterPorIdAsync(request.ContaId, cancellationToken)
            ?? throw new NotFoundException("Conta", request.ContaId);

        var categoria = await _categoriaRepository.ObterPorIdAsync(request.CategoriaId, cancellationToken)
            ?? throw new NotFoundException("Categoria", request.CategoriaId);

        var lancamento = Lancamento.Criar(
            request.ContaId, request.CategoriaId, request.Tipo, request.Valor, request.Data, request.Descricao, request.Recorrencia);

        if (request.Tipo == TipoLancamento.Despesa)
        {
            var totalGastoAntes = await _categoriaRepository.ObterTotalGastoNoMesAsync(
                categoria.Id, request.Data.Year, request.Data.Month, cancellationToken);

            var eventosDeAlerta = AvaliadorDeOrcamento.Avaliar(categoria, lancamento, totalGastoAntes);
            foreach (var evento in eventosDeAlerta)
            {
                lancamento.RegistrarAlertaOrcamento(evento);
            }
        }

        _lancamentoRepository.Adicionar(lancamento);

        // O SaveChanges e quem efetivamente despacha os eventos de dominio acumulados
        // acima (ver ApplicationDbContext), so depois que a transacao foi confirmada.
        await _unitOfWork.SalvarAlteracoesAsync(cancellationToken);

        return new LancamentoDto(
            lancamento.Id, conta.Id, conta.Nome, categoria.Id, categoria.Nome,
            lancamento.Tipo, lancamento.Valor, lancamento.Data, lancamento.Descricao, lancamento.Recorrencia);
    }
}
