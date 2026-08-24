using ContaCerta.Domain.Entities;
using ContaCerta.Domain.Enums;
using ContaCerta.Domain.Services;
using FluentAssertions;
using Xunit;

namespace ContaCerta.Domain.Tests.Services;

public class ProjetorDeFluxoDeCaixaTests
{
    private static readonly Guid ContaId = Guid.NewGuid();
    private static readonly Guid CategoriaId = Guid.NewGuid();

    [Fact]
    public void Projetar_SemLancamentosRecorrentes_SaldoProjetadoDeveSerIgualAoAtual()
    {
        var referencia = new DateOnly(2026, 8, 23);

        var projecao = ProjetorDeFluxoDeCaixa.Projetar(1000m, Array.Empty<Lancamento>(), referencia);

        projecao.SaldoProjetado.Should().Be(1000m);
        projecao.Pontos.Should().HaveCount(30);
        projecao.Pontos.Should().OnlyContain(p => p.Saldo == 1000m);
    }

    [Fact]
    public void Projetar_ComReceitaRecorrenteDentroDaJanela_DeveSomarNoSaldoProjetado()
    {
        var referencia = new DateOnly(2026, 8, 1);
        var receita = Lancamento.Criar(
            ContaId, CategoriaId, TipoLancamento.Receita, 3000m, new DateOnly(2026, 7, 5), "Contrato mensal cliente", Recorrencia.Mensal);

        var projecao = ProjetorDeFluxoDeCaixa.Projetar(1000m, new[] { receita }, referencia);

        projecao.SaldoProjetado.Should().Be(4000m);
        projecao.TotalReceitasPrevistas.Should().Be(3000m);
        projecao.TotalDespesasPrevistas.Should().Be(0m);
    }

    [Fact]
    public void Projetar_ComDespesaRecorrente_DeveSubtrairDoSaldoNaDataCorreta()
    {
        var referencia = new DateOnly(2026, 8, 1);
        var aluguel = Lancamento.Criar(
            ContaId, CategoriaId, TipoLancamento.Despesa, 1500m, new DateOnly(2026, 6, 10), "Aluguel do escritorio", Recorrencia.Mensal);

        var projecao = ProjetorDeFluxoDeCaixa.Projetar(5000m, new[] { aluguel }, referencia);

        projecao.SaldoProjetado.Should().Be(3500m);
        projecao.TotalDespesasPrevistas.Should().Be(1500m);

        var pontoAntesDoVencimento = projecao.Pontos.Single(p => p.Data == new DateOnly(2026, 8, 9));
        var pontoNoVencimento = projecao.Pontos.Single(p => p.Data == new DateOnly(2026, 8, 10));

        pontoAntesDoVencimento.Saldo.Should().Be(5000m);
        pontoNoVencimento.Saldo.Should().Be(3500m);
    }

    [Fact]
    public void Projetar_LancamentoNaoRecorrente_NuncaEntraNaProjecao()
    {
        var referencia = new DateOnly(2026, 8, 1);
        var compraAvulsa = Lancamento.Criar(
            ContaId, CategoriaId, TipoLancamento.Despesa, 999m, new DateOnly(2026, 8, 10), "Compra unica");

        var projecao = ProjetorDeFluxoDeCaixa.Projetar(1000m, new[] { compraAvulsa }, referencia);

        projecao.SaldoProjetado.Should().Be(1000m);
    }

    [Fact]
    public void Projetar_ComReceitaEDespesaRecorrentesNoMesmoDia_DeveCompensarOsValores()
    {
        var referencia = new DateOnly(2026, 8, 1);
        var receita = Lancamento.Criar(
            ContaId, CategoriaId, TipoLancamento.Receita, 2000m, new DateOnly(2026, 7, 20), "Recebimento fixo", Recorrencia.Mensal);
        var despesa = Lancamento.Criar(
            ContaId, CategoriaId, TipoLancamento.Despesa, 800m, new DateOnly(2026, 7, 20), "Assinatura de sistema", Recorrencia.Mensal);

        var projecao = ProjetorDeFluxoDeCaixa.Projetar(0m, new[] { receita, despesa }, referencia);

        projecao.SaldoProjetado.Should().Be(1200m);
    }
}
