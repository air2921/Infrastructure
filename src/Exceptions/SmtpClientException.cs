using Infrastructure.Exceptions.Global;

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
}
