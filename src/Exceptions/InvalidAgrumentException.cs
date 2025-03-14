using Infrastructure.Exceptions.Global;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.ExceptionServices;

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

    [DoesNotReturn]
    public override void ThrowNoStackTrace(string message)
        => throw ExceptionDispatchInfo.Capture(new InvalidAgrumentException(message)).SourceException;

    [DoesNotReturn]
    public override void ThrowWithStackTrace(Exception exception)
        => ExceptionDispatchInfo.Capture(exception).Throw();
}
