using FluentValidation.Results;

namespace ContaCerta.Application.Common.Exceptions;

/// <summary>
/// Falha de validacao de entrada (payload mal formado, campo obrigatorio ausente).
/// Nome propositalmente diferente de FluentValidation.ValidationException para nao
/// colidir com o tipo da propria lib nem com System.ComponentModel.DataAnnotations.
/// A API traduz isso para 400.
/// </summary>
public class ValidationAppException : Exception
{
    public IDictionary<string, string[]> Erros { get; }

    public ValidationAppException() : base("Um ou mais erros de validacao ocorreram.")
    {
        Erros = new Dictionary<string, string[]>();
    }

    public ValidationAppException(IEnumerable<ValidationFailure> falhas) : this()
    {
        Erros = falhas
            .GroupBy(f => f.PropertyName, f => f.ErrorMessage)
            .ToDictionary(g => g.Key, g => g.ToArray());
    }
}
