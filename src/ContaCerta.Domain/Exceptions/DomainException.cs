namespace ContaCerta.Domain.Exceptions;

/// <summary>
/// Excecao base para violacao de regra de negocio. A camada de API traduz isso para
/// 422 Unprocessable Entity, distinto de erro de validacao de payload (400) e de
/// recurso inexistente (404).
/// </summary>
public class DomainException : Exception
{
    public DomainException(string message) : base(message)
    {
    }
}
