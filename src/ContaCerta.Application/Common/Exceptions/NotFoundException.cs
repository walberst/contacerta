namespace ContaCerta.Application.Common.Exceptions;

/// <summary>
/// Recurso referenciado nao existe. A API traduz isso para 404.
/// </summary>
public class NotFoundException : Exception
{
    public NotFoundException(string entidade, object chave)
        : base($"{entidade} com identificador '{chave}' nao foi encontrado(a).")
    {
    }
}
