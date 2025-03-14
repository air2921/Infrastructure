using Infrastructure.Exceptions.Global;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.ExceptionServices;

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

    [DoesNotReturn]
    public override void ThrowNoStackTrace(string message)
        => throw ExceptionDispatchInfo.Capture(new CryptographyException(message)).SourceException;

    [DoesNotReturn]
    public override void ThrowWithStackTrace(Exception exception)
        => ExceptionDispatchInfo.Capture(exception).Throw();
}
