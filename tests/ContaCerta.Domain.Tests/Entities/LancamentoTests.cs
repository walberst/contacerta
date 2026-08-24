using ContaCerta.Domain.Entities;
using ContaCerta.Domain.Enums;
using ContaCerta.Domain.Exceptions;
using FluentAssertions;
using Xunit;

namespace ContaCerta.Domain.Tests.Entities;

public class LancamentoTests
{
    private static readonly Guid ContaId = Guid.NewGuid();
    private static readonly Guid CategoriaId = Guid.NewGuid();

    [Fact]
    public void Criar_ComValorZeroOuNegativo_DeveLancarExcecao()
    {
        var acao = () => Lancamento.Criar(
            ContaId, CategoriaId, TipoLancamento.Despesa, 0m, DateOnly.FromDateTime(DateTime.Today), "Teste");

        acao.Should().Throw<ValorInvalidoException>();
    }

    [Fact]
    public void Criar_SemDescricao_DeveLancarExcecao()
    {
        var acao = () => Lancamento.Criar(
            ContaId, CategoriaId, TipoLancamento.Despesa, 100m, DateOnly.FromDateTime(DateTime.Today), "  ");

        acao.Should().Throw<ValorInvalidoException>();
    }

    [Fact]
    public void Criar_SemRecorrencia_EhRecorrenteDeveSerFalso()
    {
        var lancamento = Lancamento.Criar(
            ContaId, CategoriaId, TipoLancamento.Despesa, 100m, DateOnly.FromDateTime(DateTime.Today), "Compra avulsa");

        lancamento.EhRecorrente.Should().BeFalse();
    }

    [Fact]
    public void OcorrenciasEntre_LancamentoNaoRecorrente_NuncaGeraOcorrencia()
    {
        var data = new DateOnly(2026, 1, 10);
        var lancamento = Lancamento.Criar(ContaId, CategoriaId, TipoLancamento.Despesa, 100m, data, "Compra avulsa");

        var ocorrencias = lancamento.OcorrenciasEntre(data, data.AddDays(60));

        ocorrencias.Should().BeEmpty();
    }

    [Fact]
    public void OcorrenciasEntre_LancamentoRecorrenteMensal_DeveRepetirNoMesmoDiaDoMes()
    {
        var dataOriginal = new DateOnly(2026, 1, 15);
        var lancamento = Lancamento.Criar(
            ContaId, CategoriaId, TipoLancamento.Despesa, 200m, dataOriginal, "Aluguel", Recorrencia.Mensal);

        var referencia = new DateOnly(2026, 1, 20);
        var ocorrencias = lancamento.OcorrenciasEntre(referencia, referencia.AddDays(30)).ToList();

        ocorrencias.Should().ContainSingle().Which.Should().Be(new DateOnly(2026, 2, 15));
    }

    [Fact]
    public void OcorrenciasEntre_JanelaDeVariosMeses_DeveRetornarUmaOcorrenciaPorMes()
    {
        var dataOriginal = new DateOnly(2026, 1, 1);
        var lancamento = Lancamento.Criar(
            ContaId, CategoriaId, TipoLancamento.Receita, 500m, dataOriginal, "Assinatura cliente X", Recorrencia.Mensal);

        var referencia = new DateOnly(2026, 1, 1);
        var ocorrencias = lancamento.OcorrenciasEntre(referencia, referencia.AddDays(95)).ToList();

        ocorrencias.Should().BeEquivalentTo(new[]
        {
            new DateOnly(2026, 2, 1),
            new DateOnly(2026, 3, 1),
            new DateOnly(2026, 4, 1)
        });
    }
}
