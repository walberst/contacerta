using ContaCerta.Domain.Entities;
using ContaCerta.Domain.Enums;
using ContaCerta.Domain.Services;
using FluentAssertions;
using Xunit;

namespace ContaCerta.Domain.Tests.Services;

public class AvaliadorDeOrcamentoTests
{
    private static readonly Guid ContaId = Guid.NewGuid();
    private static readonly Guid CategoriaId = Guid.NewGuid();

    [Fact]
    public void Avaliar_DespesaQueCruzaOitentaPorCento_DeveGerarEventoDeAlerta()
    {
        var categoria = Categoria.Criar("Fornecedores", TipoLancamento.Despesa, 1000m);
        var lancamento = Lancamento.Criar(
            ContaId, categoria.Id, TipoLancamento.Despesa, 300m, DateOnly.FromDateTime(DateTime.Today), "Compra de insumos");

        var eventos = AvaliadorDeOrcamento.Avaliar(categoria, lancamento, totalGastoAntesDoLancamento: 550m);

        eventos.Should().ContainSingle();
        eventos.Single().PercentualLimite.Should().Be(80);
        eventos.Single().TotalGastoNoMes.Should().Be(850m);
    }

    [Fact]
    public void Avaliar_Receita_NuncaDeveGerarAlerta()
    {
        var categoria = Categoria.Criar("Vendas", TipoLancamento.Receita, 1000m);
        var lancamento = Lancamento.Criar(
            ContaId, categoria.Id, TipoLancamento.Receita, 5000m, DateOnly.FromDateTime(DateTime.Today), "Venda avulsa");

        var eventos = AvaliadorDeOrcamento.Avaliar(categoria, lancamento, totalGastoAntesDoLancamento: 0m);

        eventos.Should().BeEmpty();
    }

    [Fact]
    public void Avaliar_DespesaQueNaoCruzaLimiteAlgum_NaoDeveGerarEvento()
    {
        var categoria = Categoria.Criar("Fornecedores", TipoLancamento.Despesa, 1000m);
        var lancamento = Lancamento.Criar(
            ContaId, categoria.Id, TipoLancamento.Despesa, 50m, DateOnly.FromDateTime(DateTime.Today), "Compra pequena");

        var eventos = AvaliadorDeOrcamento.Avaliar(categoria, lancamento, totalGastoAntesDoLancamento: 100m);

        eventos.Should().BeEmpty();
    }

    [Fact]
    public void Avaliar_DespesaGrandeQueUltrapassaVariosLimitesDeUmaVez_DeveGerarUmEventoPorLimite()
    {
        var categoria = Categoria.Criar("Fornecedores", TipoLancamento.Despesa, 1000m);
        var lancamento = Lancamento.Criar(
            ContaId, categoria.Id, TipoLancamento.Despesa, 700m, DateOnly.FromDateTime(DateTime.Today), "Compra grande");

        var eventos = AvaliadorDeOrcamento.Avaliar(categoria, lancamento, totalGastoAntesDoLancamento: 500m);

        eventos.Select(e => e.PercentualLimite).Should().BeEquivalentTo(new[] { 80, 100 });
        eventos.Should().OnlyContain(e => e.CategoriaId == categoria.Id && e.ContaId == ContaId);
    }
}
