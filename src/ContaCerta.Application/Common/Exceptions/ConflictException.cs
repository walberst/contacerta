namespace ContaCerta.Application.Common.Exceptions;

/// <summary>
/// Operacao esbarra em um estado ja existente que impede a acao (ex: nome de conta
/// duplicado). A API traduz isso para 409.
/// </summary>
public class ConflictException : Exception
{
    public ConflictException(string message) : base(message)
    {
    }
}
