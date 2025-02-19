using Infrastructure.Exceptions.Global;

namespace Infrastructure.Exceptions;

public class CryptographyException : InfrastructureException
{
    public CryptographyException() : base()
    {

    }

    public CryptographyException(string? message) : base(message)
    {

    }

    public CryptographyException(string? message, Exception? exception) : base(message, exception)
    {

    }
}
