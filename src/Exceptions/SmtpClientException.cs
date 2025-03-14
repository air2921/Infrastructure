using Infrastructure.Exceptions.Global;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.ExceptionServices;

namespace Infrastructure.Exceptions;

public class SmtpClientException : InfrastructureException
{
    public SmtpClientException() : base()
    {

    }

    public SmtpClientException(string? message) : base(message)
    {

    }

    public SmtpClientException(string? message, Exception? exception) : base(message, exception)
    {

    }

    [DoesNotReturn]
    public override void ThrowNoStackTrace(string message)
        => throw ExceptionDispatchInfo.Capture(new SmtpClientException(message)).SourceException;

    [DoesNotReturn]
    public override void ThrowWithStackTrace(Exception exception)
        => ExceptionDispatchInfo.Capture(exception).Throw();
}
