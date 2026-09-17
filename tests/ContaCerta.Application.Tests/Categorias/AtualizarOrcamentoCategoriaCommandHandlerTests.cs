using ContaCerta.Application.Categorias.Commands;
using ContaCerta.Application.Common.Exceptions;
using ContaCerta.Domain.Entities;
using ContaCerta.Domain.Enums;
using ContaCerta.Domain.Interfaces;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace ContaCerta.Application.Tests.Categorias;

public class AtualizarOrcamentoCategoriaCommandHandlerTests
{
    private readonly ICategoriaRepository _categoriaRepository = Substitute.For<ICategoriaRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();

    private AtualizarOrcamentoCategoriaCommandHandler CriarHandler() => new(_categoriaRepository, _unitOfWork);

    [Fact]
    public async Task Handle_CategoriaInexistente_DeveLancarNotFoundException()
    {
        _categoriaRepository.ObterPorIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((Categoria?)null);

        var acao = () => CriarHandler().Handle(
            new AtualizarOrcamentoCategoriaCommand(Guid.NewGuid(), 500m), CancellationToken.None);

        await acao.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_CategoriaExistente_DeveAtualizarOrcamentoESalvar()
    {
        var categoria = Categoria.Criar("Fornecedores", TipoLancamento.Despesa, 1000m);
        _categoriaRepository.ObterPorIdAsync(categoria.Id, Arg.Any<CancellationToken>()).Returns(categoria);

        await CriarHandler().Handle(new AtualizarOrcamentoCategoriaCommand(categoria.Id, 2000m), CancellationToken.None);

        categoria.OrcamentoMensal.Should().Be(2000m);
        await _unitOfWork.Received(1).SalvarAlteracoesAsync(Arg.Any<CancellationToken>());
    }
}
