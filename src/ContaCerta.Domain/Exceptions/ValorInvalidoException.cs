namespace ContaCerta.Domain.Exceptions;

public class ValorInvalidoException : DomainException
{
    public ValorInvalidoException(string message) : base(message)
    {
    }
}
