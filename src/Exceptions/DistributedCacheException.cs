using Infrastructure.Exceptions.Global;
using System.Diagnostics.CodeAnalysis;

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

    public static void ThrowIf([DoesNotReturnIf(true)] bool condition, string message)
    {
        if (!condition)
            return;

        throw new DistributedCacheException(message);
    }

    public static void ThrowIfNull([NotNull] object? param, string message)
    {
        if (param is null)
            throw new DistributedCacheException(message);
    }
}
