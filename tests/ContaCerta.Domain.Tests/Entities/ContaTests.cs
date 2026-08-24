using ContaCerta.Domain.Entities;
using ContaCerta.Domain.Enums;
using ContaCerta.Domain.Exceptions;
using FluentAssertions;
using Xunit;

namespace ContaCerta.Domain.Tests.Entities;

public class ContaTests
{
    [Fact]
    public void Criar_ComDadosValidos_DevePreencherPropriedades()
    {
        var conta = Conta.Criar("Conta PJ Nubank", TipoConta.Negocio, 1000m);

        conta.Nome.Should().Be("Conta PJ Nubank");
        conta.Tipo.Should().Be(TipoConta.Negocio);
        conta.SaldoInicial.Should().Be(1000m);
        conta.Id.Should().NotBeEmpty();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Criar_SemNome_DeveLancarExcecao(string nomeInvalido)
    {
        var acao = () => Conta.Criar(nomeInvalido, TipoConta.Pessoal, 0m);

        acao.Should().Throw<ValorInvalidoException>();
    }

    [Fact]
    public void Criar_ComSaldoInicialNegativo_DevePermitir()
    {
        // Saldo inicial negativo representa conta que ja comeca no vermelho
        // (ex: cartao ja parcelado), cenario real e valido para o MEI.
        var conta = Conta.Criar("Conta com divida", TipoConta.Pessoal, -500m);

        conta.SaldoInicial.Should().Be(-500m);
    }

    [Fact]
    public void Renomear_ComNomeValido_DeveAtualizar()
    {
        var conta = Conta.Criar("Nome antigo", TipoConta.Pessoal, 0m);

        conta.Renomear("Nome novo");

        conta.Nome.Should().Be("Nome novo");
    }

    [Fact]
    public void Renomear_ComNomeVazio_DeveLancarExcecao()
    {
        var conta = Conta.Criar("Nome antigo", TipoConta.Pessoal, 0m);

        var acao = () => conta.Renomear(" ");

        acao.Should().Throw<ValorInvalidoException>();
    }
}
