using ContaCerta.Application.Common.Interfaces;
using ContaCerta.Application.Dashboard.Queries;
using ContaCerta.Domain.Entities;
using ContaCerta.Domain.Enums;
using ContaCerta.Domain.Interfaces;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace ContaCerta.Application.Tests.Dashboard;

public class ObterResumoMensalQueryHandlerTests
{
    private readonly ILancamentoRepository _lancamentoRepository = Substitute.For<ILancamentoRepository>();
    private readonly IContaRepository _contaRepository = Substitute.For<IContaRepository>();
    private readonly IDateTimeProvider _dateTimeProvider = Substitute.For<IDateTimeProvider>();

    [Fact]
    public async Task Handle_ComLancamentosDeAmbosOsTipos_DeveSepararPessoalDeNegocio()
    {
        var hoje = new DateOnly(2026, 8, 15);
        _dateTimeProvider.Hoje.Returns(hoje);

        var contaNegocio = Conta.Criar("PJ", TipoConta.Negocio, 0m);
        var contaPessoal = Conta.Criar("Pessoal", TipoConta.Pessoal, 0m);
        _contaRepository.ListarAsync(Arg.Any<CancellationToken>()).Returns(new List<Conta> { contaNegocio, contaPessoal });

        var categoriaVendas = Categoria.Criar("Vendas", TipoLancamento.Receita, 0m);
        var categoriaFornecedores = Categoria.Criar("Fornecedores", TipoLancamento.Despesa, 0m);
        var categoriaAlimentacao = Categoria.Criar("Alimentacao", TipoLancamento.Despesa, 0m);

        var lancamentos = new List<Lancamento>
        {
            Lancamento.Criar(contaNegocio.Id, categoriaVendas.Id, TipoLancamento.Receita, 5000m, hoje, "Venda"),
            Lancamento.Criar(contaNegocio.Id, categoriaFornecedores.Id, TipoLancamento.Despesa, 2000m, hoje, "Compra"),
            Lancamento.Criar(contaPessoal.Id, categoriaAlimentacao.Id, TipoLancamento.Despesa, 800m, hoje, "Mercado")
        };
        _lancamentoRepository
            .ListarPorPeriodoAsync(Arg.Any<DateOnly>(), Arg.Any<DateOnly>(), Arg.Any<CancellationToken>())
            .Returns(lancamentos);

        var handler = new ObterResumoMensalQueryHandler(_lancamentoRepository, _contaRepository, _dateTimeProvider);
        var resumo = await handler.Handle(new ObterResumoMensalQuery(), CancellationToken.None);

        resumo.Negocio.TotalReceitas.Should().Be(5000m);
        resumo.Negocio.TotalDespesas.Should().Be(2000m);
        resumo.Negocio.Saldo.Should().Be(3000m);

        resumo.Pessoal.TotalReceitas.Should().Be(0m);
        resumo.Pessoal.TotalDespesas.Should().Be(800m);
        resumo.Pessoal.Saldo.Should().Be(-800m);

        resumo.Consolidado.Saldo.Should().Be(2200m);
    }
}
