using Infrastructure.Exceptions.Global;
using System.Diagnostics.CodeAnalysis;

namespace Infrastructure.Exceptions;

public class EntityException : InfrastructureException
{
    public EntityException() : base()
    {

    }

    public EntityException(string? message) : base(message)
    {

    }

    public EntityException(string? message, Exception? exception) : base(message, exception)
    {

    }

    public static void ThrowIf([DoesNotReturnIf(true)] bool condition, string message)
    {
        if (!condition)
            return;

        throw new EntityException(message);
    }

    public static void ThrowIfNull([NotNull] object? param, string message)
    {
        if (param is null)
            throw new EntityException(message);
    }
}
