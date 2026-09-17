using ContaCerta.Application.Common.Exceptions;
using ContaCerta.Application.Lancamentos.Commands;
using ContaCerta.Domain.Entities;
using ContaCerta.Domain.Enums;
using ContaCerta.Domain.Events;
using ContaCerta.Domain.Interfaces;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace ContaCerta.Application.Tests.Lancamentos;

public class CriarLancamentoCommandHandlerTests
{
    private readonly IContaRepository _contaRepository = Substitute.For<IContaRepository>();
    private readonly ICategoriaRepository _categoriaRepository = Substitute.For<ICategoriaRepository>();
    private readonly ILancamentoRepository _lancamentoRepository = Substitute.For<ILancamentoRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();

    private CriarLancamentoCommandHandler CriarHandler() =>
        new(_contaRepository, _categoriaRepository, _lancamentoRepository, _unitOfWork);

    [Fact]
    public async Task Handle_ContaInexistente_DeveLancarNotFoundException()
    {
        _contaRepository.ObterPorIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((Conta?)null);

        var comando = new CriarLancamentoCommand(
            Guid.NewGuid(), Guid.NewGuid(), TipoLancamento.Despesa, 100m,
            DateOnly.FromDateTime(DateTime.Today), "Teste", Recorrencia.Nenhuma);

        var acao = () => CriarHandler().Handle(comando, CancellationToken.None);

        await acao.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_CategoriaInexistente_DeveLancarNotFoundException()
    {
        var conta = Conta.Criar("Conta PJ", TipoConta.Negocio, 0m);
        _contaRepository.ObterPorIdAsync(conta.Id, Arg.Any<CancellationToken>()).Returns(conta);
        _categoriaRepository.ObterPorIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((Categoria?)null);

        var comando = new CriarLancamentoCommand(
            conta.Id, Guid.NewGuid(), TipoLancamento.Despesa, 100m,
            DateOnly.FromDateTime(DateTime.Today), "Teste", Recorrencia.Nenhuma);

        var acao = () => CriarHandler().Handle(comando, CancellationToken.None);

        await acao.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_DespesaQueCruzaLimiteDeOrcamento_DeveRegistrarEventoNoLancamentoAntesDeSalvar()
    {
        var conta = Conta.Criar("Conta PJ", TipoConta.Negocio, 0m);
        var categoria = Categoria.Criar("Fornecedores", TipoLancamento.Despesa, 1000m);

        _contaRepository.ObterPorIdAsync(conta.Id, Arg.Any<CancellationToken>()).Returns(conta);
        _categoriaRepository.ObterPorIdAsync(categoria.Id, Arg.Any<CancellationToken>()).Returns(categoria);
        _categoriaRepository
            .ObterTotalGastoNoMesAsync(categoria.Id, Arg.Any<int>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(750m);

        Lancamento? capturado = null;
        _lancamentoRepository.When(r => r.Adicionar(Arg.Any<Lancamento>())).Do(call => capturado = call.Arg<Lancamento>());

        var comando = new CriarLancamentoCommand(
            conta.Id, categoria.Id, TipoLancamento.Despesa, 100m,
            DateOnly.FromDateTime(DateTime.Today), "Compra grande", Recorrencia.Nenhuma);

        await CriarHandler().Handle(comando, CancellationToken.None);

        capturado.Should().NotBeNull();
        capturado!.DomainEvents.Should().ContainSingle();
        var evento = capturado.DomainEvents.Single().Should().BeOfType<LimiteOrcamentoAtingidoEvent>().Subject;
        evento.PercentualLimite.Should().Be(80);
        evento.TotalGastoNoMes.Should().Be(850m);

        await _unitOfWork.Received(1).SalvarAlteracoesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ReceitaAcimaDoOrcamentoDaCategoria_NuncaDeveRegistrarEvento()
    {
        var conta = Conta.Criar("Conta PJ", TipoConta.Negocio, 0m);
        var categoria = Categoria.Criar("Vendas", TipoLancamento.Receita, 1000m);

        _contaRepository.ObterPorIdAsync(conta.Id, Arg.Any<CancellationToken>()).Returns(conta);
        _categoriaRepository.ObterPorIdAsync(categoria.Id, Arg.Any<CancellationToken>()).Returns(categoria);

        Lancamento? capturado = null;
        _lancamentoRepository.When(r => r.Adicionar(Arg.Any<Lancamento>())).Do(call => capturado = call.Arg<Lancamento>());

        var comando = new CriarLancamentoCommand(
            conta.Id, categoria.Id, TipoLancamento.Receita, 5000m,
            DateOnly.FromDateTime(DateTime.Today), "Venda grande", Recorrencia.Nenhuma);

        await CriarHandler().Handle(comando, CancellationToken.None);

        capturado!.DomainEvents.Should().BeEmpty();
        await _categoriaRepository.DidNotReceive().ObterTotalGastoNoMesAsync(
            Arg.Any<Guid>(), Arg.Any<int>(), Arg.Any<int>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_DespesaQueNaoCruzaLimite_NaoDeveRegistrarEvento()
    {
        var conta = Conta.Criar("Conta PJ", TipoConta.Negocio, 0m);
        var categoria = Categoria.Criar("Fornecedores", TipoLancamento.Despesa, 1000m);

        _contaRepository.ObterPorIdAsync(conta.Id, Arg.Any<CancellationToken>()).Returns(conta);
        _categoriaRepository.ObterPorIdAsync(categoria.Id, Arg.Any<CancellationToken>()).Returns(categoria);
        _categoriaRepository
            .ObterTotalGastoNoMesAsync(categoria.Id, Arg.Any<int>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(100m);

        Lancamento? capturado = null;
        _lancamentoRepository.When(r => r.Adicionar(Arg.Any<Lancamento>())).Do(call => capturado = call.Arg<Lancamento>());

        var comando = new CriarLancamentoCommand(
            conta.Id, categoria.Id, TipoLancamento.Despesa, 50m,
            DateOnly.FromDateTime(DateTime.Today), "Compra pequena", Recorrencia.Nenhuma);

        await CriarHandler().Handle(comando, CancellationToken.None);

        capturado!.DomainEvents.Should().BeEmpty();
    }
}
