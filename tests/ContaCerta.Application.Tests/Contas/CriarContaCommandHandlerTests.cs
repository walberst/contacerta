using ContaCerta.Application.Common.Exceptions;
using ContaCerta.Application.Contas.Commands;
using ContaCerta.Domain.Entities;
using ContaCerta.Domain.Enums;
using ContaCerta.Domain.Interfaces;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace ContaCerta.Application.Tests.Contas;

public class CriarContaCommandHandlerTests
{
    private readonly IContaRepository _contaRepository = Substitute.For<IContaRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();

    private CriarContaCommandHandler CriarHandler() => new(_contaRepository, _unitOfWork);

    [Fact]
    public async Task Handle_ComNomeInedito_DeveCriarESalvar()
    {
        _contaRepository.ListarAsync(Arg.Any<CancellationToken>()).Returns(new List<Conta>());

        var resultado = await CriarHandler().Handle(
            new CriarContaCommand("Conta PJ", TipoConta.Negocio, 1000m), CancellationToken.None);

        resultado.Nome.Should().Be("Conta PJ");
        resultado.SaldoAtual.Should().Be(1000m);
        _contaRepository.Received(1).Adicionar(Arg.Any<Conta>());
        await _unitOfWork.Received(1).SalvarAlteracoesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ComNomeEDupostoJaExistente_DeveLancarConflictException()
    {
        var existente = Conta.Criar("Conta PJ", TipoConta.Negocio, 0m);
        _contaRepository.ListarAsync(Arg.Any<CancellationToken>()).Returns(new List<Conta> { existente });

        var acao = () => CriarHandler().Handle(new CriarContaCommand("Conta PJ", TipoConta.Negocio, 500m), CancellationToken.None);

        await acao.Should().ThrowAsync<ConflictException>();
        _contaRepository.DidNotReceive().Adicionar(Arg.Any<Conta>());
    }

    [Fact]
    public async Task Handle_MesmoNomeMasTipoDiferente_DevePermitir()
    {
        var existente = Conta.Criar("Conta principal", TipoConta.Negocio, 0m);
        _contaRepository.ListarAsync(Arg.Any<CancellationToken>()).Returns(new List<Conta> { existente });

        var resultado = await CriarHandler().Handle(
            new CriarContaCommand("Conta principal", TipoConta.Pessoal, 0m), CancellationToken.None);

        resultado.Tipo.Should().Be(TipoConta.Pessoal);
    }
}
