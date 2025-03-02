using Infrastructure.Exceptions.Global;

namespace Infrastructure.Exceptions;

public class InvalidAgrumentException : InfrastructureException
{
    public InvalidAgrumentException() : base()
    {

    }

    public InvalidAgrumentException(string? message) : base(message)
    {

    }

    public InvalidAgrumentException(string? message, Exception? exception) : base(message, exception)
    {

    }
}
