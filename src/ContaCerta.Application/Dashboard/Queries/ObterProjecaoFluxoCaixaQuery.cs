using ContaCerta.Application.Common.Interfaces;
using ContaCerta.Domain.Interfaces;
using ContaCerta.Domain.Services;
using MediatR;

namespace ContaCerta.Application.Dashboard.Queries;

/// <summary>
/// Quando ContaId e nulo, projeta o consolidado de todas as contas. Passar um
/// ContaId especifico projeta so aquela conta, util para o MEI olhar so o negocio
/// ou so o pessoal isoladamente.
/// </summary>
public sealed record ObterProjecaoFluxoCaixaQuery(Guid? ContaId) : IRequest<ProjecaoFluxoCaixa>;

public class ObterProjecaoFluxoCaixaQueryHandler : IRequestHandler<ObterProjecaoFluxoCaixaQuery, ProjecaoFluxoCaixa>
{
    private readonly IContaRepository _contaRepository;
    private readonly ILancamentoRepository _lancamentoRepository;
    private readonly IDateTimeProvider _dateTimeProvider;

    public ObterProjecaoFluxoCaixaQueryHandler(
        IContaRepository contaRepository,
        ILancamentoRepository lancamentoRepository,
        IDateTimeProvider dateTimeProvider)
    {
        _contaRepository = contaRepository;
        _lancamentoRepository = lancamentoRepository;
        _dateTimeProvider = dateTimeProvider;
    }

    public async Task<ProjecaoFluxoCaixa> Handle(ObterProjecaoFluxoCaixaQuery request, CancellationToken cancellationToken)
    {
        decimal saldoAtual;
        if (request.ContaId is { } contaId)
        {
            saldoAtual = await _contaRepository.ObterSaldoAtualAsync(contaId, cancellationToken);
        }
        else
        {
            var contas = await _contaRepository.ListarAsync(cancellationToken);
            saldoAtual = 0m;
            foreach (var conta in contas)
            {
                saldoAtual += await _contaRepository.ObterSaldoAtualAsync(conta.Id, cancellationToken);
            }
        }

        var lancamentosRecorrentes = await _lancamentoRepository.ListarRecorrentesAsync(request.ContaId, cancellationToken);

        return ProjetorDeFluxoDeCaixa.Projetar(saldoAtual, lancamentosRecorrentes, _dateTimeProvider.Hoje);
    }
}
