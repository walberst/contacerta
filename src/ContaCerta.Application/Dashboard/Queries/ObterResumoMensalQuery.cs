using ContaCerta.Application.Common.Interfaces;
using ContaCerta.Domain.Entities;
using ContaCerta.Domain.Enums;
using ContaCerta.Domain.Interfaces;
using MediatR;

namespace ContaCerta.Application.Dashboard.Queries;

public sealed record ObterResumoMensalQuery : IRequest<ResumoMensalDto>;

public class ObterResumoMensalQueryHandler : IRequestHandler<ObterResumoMensalQuery, ResumoMensalDto>
{
    private readonly ILancamentoRepository _lancamentoRepository;
    private readonly IContaRepository _contaRepository;
    private readonly IDateTimeProvider _dateTimeProvider;

    public ObterResumoMensalQueryHandler(
        ILancamentoRepository lancamentoRepository,
        IContaRepository contaRepository,
        IDateTimeProvider dateTimeProvider)
    {
        _lancamentoRepository = lancamentoRepository;
        _contaRepository = contaRepository;
        _dateTimeProvider = dateTimeProvider;
    }

    public async Task<ResumoMensalDto> Handle(ObterResumoMensalQuery request, CancellationToken cancellationToken)
    {
        var hoje = _dateTimeProvider.Hoje;
        var inicioDoMes = new DateOnly(hoje.Year, hoje.Month, 1);
        var fimDoMes = inicioDoMes.AddMonths(1).AddDays(-1);

        var lancamentosDoMes = await _lancamentoRepository.ListarPorPeriodoAsync(inicioDoMes, fimDoMes, cancellationToken);
        var contas = await _contaRepository.ListarAsync(cancellationToken);
        var tipoPorConta = contas.ToDictionary(c => c.Id, c => c.Tipo);

        var pessoal = Resumir(lancamentosDoMes, tipoPorConta, TipoConta.Pessoal);
        var negocio = Resumir(lancamentosDoMes, tipoPorConta, TipoConta.Negocio);
        var consolidado = new ResumoPorTipoContaDto(
            pessoal.TotalReceitas + negocio.TotalReceitas,
            pessoal.TotalDespesas + negocio.TotalDespesas,
            pessoal.Saldo + negocio.Saldo);

        return new ResumoMensalDto(hoje.Year, hoje.Month, pessoal, negocio, consolidado);
    }

    private static ResumoPorTipoContaDto Resumir(
        IReadOnlyList<Lancamento> lancamentos, IReadOnlyDictionary<Guid, TipoConta> tipoPorConta, TipoConta tipo)
    {
        var doTipo = lancamentos.Where(l => tipoPorConta.TryGetValue(l.ContaId, out var t) && t == tipo).ToList();

        var receitas = doTipo.Where(l => l.Tipo == TipoLancamento.Receita).Sum(l => l.Valor);
        var despesas = doTipo.Where(l => l.Tipo == TipoLancamento.Despesa).Sum(l => l.Valor);

        return new ResumoPorTipoContaDto(receitas, despesas, receitas - despesas);
    }
}
