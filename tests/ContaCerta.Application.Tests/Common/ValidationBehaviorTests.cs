using ContaCerta.Application.Common.Behaviors;
using ContaCerta.Application.Common.Exceptions;
using FluentAssertions;
using FluentValidation;
using MediatR;
using Xunit;

namespace ContaCerta.Application.Tests.Common;

public sealed record ComandoFalso(string Nome) : IRequest<string>;

public class ComandoFalsoValidator : AbstractValidator<ComandoFalso>
{
    public ComandoFalsoValidator()
    {
        RuleFor(c => c.Nome).NotEmpty();
    }
}

public class ValidationBehaviorTests
{
    [Fact]
    public async Task Handle_ComRequestInvalido_DeveLancarValidationAppException()
    {
        var behavior = new ValidationBehavior<ComandoFalso, string>(new[] { new ComandoFalsoValidator() });

        var acao = () => behavior.Handle(new ComandoFalso(string.Empty), () => Task.FromResult("ok"), CancellationToken.None);

        var excecao = await acao.Should().ThrowAsync<ValidationAppException>();
        excecao.Which.Erros.Should().ContainKey(nameof(ComandoFalso.Nome));
    }

    [Fact]
    public async Task Handle_ComRequestValido_DeveChamarProximoDelegate()
    {
        var behavior = new ValidationBehavior<ComandoFalso, string>(new[] { new ComandoFalsoValidator() });

        var resultado = await behavior.Handle(new ComandoFalso("Valido"), () => Task.FromResult("ok"), CancellationToken.None);

        resultado.Should().Be("ok");
    }

    [Fact]
    public async Task Handle_SemValidadoresRegistrados_DeveChamarProximoDelegateDiretamente()
    {
        var behavior = new ValidationBehavior<ComandoFalso, string>(Array.Empty<IValidator<ComandoFalso>>());

        var resultado = await behavior.Handle(new ComandoFalso(string.Empty), () => Task.FromResult("passou"), CancellationToken.None);

        resultado.Should().Be("passou");
    }
}
