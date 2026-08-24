using ContaCerta.Application.Common.Exceptions;
using FluentValidation;
using MediatR;

namespace ContaCerta.Application.Common.Behaviors;

/// <summary>
/// Roda todos os validadores FluentValidation registrados para o request antes do
/// handler executar. Centralizado aqui para que nenhum handler precise lembrar de
/// chamar validacao manualmente.
/// </summary>
public class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;

    public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators)
    {
        _validators = validators;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (!_validators.Any())
        {
            return await next();
        }

        var contexto = new ValidationContext<TRequest>(request);

        var resultados = await Task.WhenAll(
            _validators.Select(v => v.ValidateAsync(contexto, cancellationToken)));

        var falhas = resultados
            .SelectMany(r => r.Errors)
            .Where(f => f is not null)
            .ToList();

        if (falhas.Count > 0)
        {
            throw new ValidationAppException(falhas);
        }

        return await next();
    }
}
