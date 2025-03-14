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

    [DoesNotReturn]
    public override void Throw(string message)
        => throw new EntityException(message);

    public override void ThrowIf([DoesNotReturnIf(true)] bool condition, string message)
    {
        if (!condition)
            return;

        throw new EntityException(message);
    }

    public override void ThrowIfNull([NotNull] object? param, string message)
    {
        if (param is null)
            throw new EntityException(message);
    }
}
