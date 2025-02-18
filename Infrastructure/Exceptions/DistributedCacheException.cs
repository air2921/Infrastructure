using Infrastructure.Exceptions.Global;

namespace Infrastructure.Exceptions;

public class DistributedCacheException : InfrastructureException
{
    public DistributedCacheException() : base()
    {

    }

    public DistributedCacheException(string? message) : base(message)
    {

    }

    public DistributedCacheException(string? message, Exception? exception) : base(message, exception)
    {

    }
}
