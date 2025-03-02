using Infrastructure.Exceptions.Global;

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
}
