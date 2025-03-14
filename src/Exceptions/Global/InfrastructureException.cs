using System.Diagnostics.CodeAnalysis;

namespace Infrastructure.Exceptions.Global;

public abstract class InfrastructureException : Exception
{
    public InfrastructureException() : base()
    {

    }

    public InfrastructureException(string? message) : base(message)
    {

    }

    public InfrastructureException(string? message, Exception? exception) : base(message, exception)
    {

    }

    [DoesNotReturn]
    public abstract void Throw(string message);

    public abstract void ThrowIf([DoesNotReturnIf(true)] bool condition, string message);

    public abstract void ThrowIfNull([NotNull] object? param, string message);
}
