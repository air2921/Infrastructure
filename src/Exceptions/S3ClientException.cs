using Infrastructure.Exceptions.Global;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.ExceptionServices;

namespace Infrastructure.Exceptions;

public class S3ClientException : InfrastructureException
{
    public S3ClientException() : base()
    {

    }

    public S3ClientException(string? message) : base(message)
    {

    }

    public S3ClientException(string? message, Exception? exception) : base(message, exception)
    {

    }

    [DoesNotReturn]
    public override void ThrowNoStackTrace(string message)
        => throw ExceptionDispatchInfo.Capture(new S3ClientException(message)).SourceException;

    [DoesNotReturn]
    public override void ThrowWithStackTrace(Exception exception)
        => ExceptionDispatchInfo.Capture(exception).Throw();
}
