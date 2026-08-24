using ContaCerta.Domain.Entities;
using ContaCerta.Domain.Enums;
using ContaCerta.Domain.Exceptions;
using FluentAssertions;
using Xunit;

namespace ContaCerta.Domain.Tests.Entities;

public class CategoriaTests
{
    [Fact]
    public void Criar_SemPercentuaisInformados_DeveUsarPadrao80e100()
    {
        var categoria = Categoria.Criar("Fornecedores", TipoLancamento.Despesa, 1000m);

        categoria.PercentuaisAlerta.Should().BeEquivalentTo(new[] { 80, 100 });
    }

    [Fact]
    public void Criar_ComOrcamentoNegativo_DeveLancarExcecao()
    {
        var acao = () => Categoria.Criar("Aluguel", TipoLancamento.Despesa, -1m);

        acao.Should().Throw<ValorInvalidoException>();
    }

    [Theory]
    [InlineData(1000, 799, 799, false)] // ainda abaixo de 80% de 1000
    [InlineData(1000, 799, 800, true)]  // cruzou os 80% de 1000
    [InlineData(1000, 999, 1000, true)] // cruzou os 100%
    public void ObterLimitesCruzados_DeveDetectarCruzamentoDoLimite(
        decimal orcamento, decimal totalAntes, decimal totalDepois, bool esperaAlgumCruzamento)
    {
        var categoria = Categoria.Criar("Fornecedores", TipoLancamento.Despesa, orcamento);

        var cruzados = categoria.ObterLimitesCruzados(totalAntes, totalDepois);

        cruzados.Any().Should().Be(esperaAlgumCruzamento);
    }

    [Fact]
    public void ObterLimitesCruzados_QuandoJaEstourouAntes_NaoDeveRepetirAlerta()
    {
        // Categoria ja estava em 900/1000 (90%, ja tinha cruzado os 80%). Uma nova
        // despesa que leva para 950/1000 nao deve gerar novo alerta de 80%.
        var categoria = Categoria.Criar("Fornecedores", TipoLancamento.Despesa, 1000m);

        var cruzados = categoria.ObterLimitesCruzados(900m, 950m);

        cruzados.Should().BeEmpty();
    }

    [Fact]
    public void ObterLimitesCruzados_ComOrcamentoZero_NuncaDeveAlertar()
    {
        // Orcamento zero significa "sem monitoramento definido" para essa categoria.
        var categoria = Categoria.Criar("Diversos", TipoLancamento.Despesa, 0m);

        var cruzados = categoria.ObterLimitesCruzados(0m, 10_000m);

        cruzados.Should().BeEmpty();
    }

    [Fact]
    public void ObterLimitesCruzados_PulandoDeUmaVezDeAbaixoDe80ParaAcimaDe100_DeveRetornarAmbosOsLimites()
    {
        var categoria = Categoria.Criar("Fornecedores", TipoLancamento.Despesa, 1000m);

        var cruzados = categoria.ObterLimitesCruzados(500m, 1200m);

        cruzados.Should().BeEquivalentTo(new[] { 80, 100 });
    }

    [Fact]
    public void AtualizarOrcamento_ComValorNegativo_DeveLancarExcecao()
    {
        var categoria = Categoria.Criar("Fornecedores", TipoLancamento.Despesa, 1000m);

        var acao = () => categoria.AtualizarOrcamento(-1m);

        acao.Should().Throw<ValorInvalidoException>();
    }
}
