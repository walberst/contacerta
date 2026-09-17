using ContaCerta.Application.Common.Interfaces;
using ContaCerta.Application.Dashboard.Queries;
using ContaCerta.Domain.Entities;
using ContaCerta.Domain.Enums;
using ContaCerta.Domain.Interfaces;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace ContaCerta.Application.Tests.Dashboard;

public class ObterProjecaoFluxoCaixaQueryHandlerTests
{
    private readonly IContaRepository _contaRepository = Substitute.For<IContaRepository>();
    private readonly ILancamentoRepository _lancamentoRepository = Substitute.For<ILancamentoRepository>();
    private readonly IDateTimeProvider _dateTimeProvider = Substitute.For<IDateTimeProvider>();

    private ObterProjecaoFluxoCaixaQueryHandler CriarHandler() =>
        new(_contaRepository, _lancamentoRepository, _dateTimeProvider);

    [Fact]
    public async Task Handle_SemContaEspecifica_DeveSomarSaldoDeTodasAsContas()
    {
        var hoje = new DateOnly(2026, 8, 1);
        _dateTimeProvider.Hoje.Returns(hoje);

        var contaA = Conta.Criar("A", TipoConta.Negocio, 0m);
        var contaB = Conta.Criar("B", TipoConta.Pessoal, 0m);
        _contaRepository.ListarAsync(Arg.Any<CancellationToken>()).Returns(new List<Conta> { contaA, contaB });
        _contaRepository.ObterSaldoAtualAsync(contaA.Id, Arg.Any<CancellationToken>()).Returns(1000m);
        _contaRepository.ObterSaldoAtualAsync(contaB.Id, Arg.Any<CancellationToken>()).Returns(500m);
        _lancamentoRepository.ListarRecorrentesAsync(null, Arg.Any<CancellationToken>()).Returns(new List<Lancamento>());

        var projecao = await CriarHandler().Handle(new ObterProjecaoFluxoCaixaQuery(null), CancellationToken.None);

        projecao.SaldoAtual.Should().Be(1500m);
        projecao.SaldoProjetado.Should().Be(1500m);
    }

    [Fact]
    public async Task Handle_ComContaEspecifica_DeveConsultarApenasAquelaConta()
    {
        var hoje = new DateOnly(2026, 8, 1);
        _dateTimeProvider.Hoje.Returns(hoje);

        var contaId = Guid.NewGuid();
        _contaRepository.ObterSaldoAtualAsync(contaId, Arg.Any<CancellationToken>()).Returns(750m);
        _lancamentoRepository.ListarRecorrentesAsync(contaId, Arg.Any<CancellationToken>()).Returns(new List<Lancamento>());

        var projecao = await CriarHandler().Handle(new ObterProjecaoFluxoCaixaQuery(contaId), CancellationToken.None);

        projecao.SaldoAtual.Should().Be(750m);
        await _contaRepository.DidNotReceive().ListarAsync(Arg.Any<CancellationToken>());
    }
}
