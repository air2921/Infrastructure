using Infrastructure.Exceptions.Global;
using System.Diagnostics.CodeAnalysis;

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
    public override void Throw(string message)
        => throw new SmtpClientException(message);

    public override void ThrowIf([DoesNotReturnIf(true)] bool condition, string message)
    {
        if (!condition)
            return;

        throw new SmtpClientException(message);
    }

    public override void ThrowIfNull([NotNull] object? param, string message)
    {
        if (param is null)
            throw new SmtpClientException(message);
    }
}
