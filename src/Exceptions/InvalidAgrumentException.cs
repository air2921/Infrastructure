using Infrastructure.Exceptions.Global;
using System.Diagnostics.CodeAnalysis;

namespace Infrastructure.Exceptions;

public class InvalidAgrumentException : InfrastructureException
{
    public InvalidAgrumentException() : base()
    {

    }

    public InvalidAgrumentException(string? message) : base(message)
    {

    }

    public InvalidAgrumentException(string? message, Exception? exception) : base(message, exception)
    {

    }

    public static void ThrowIf([DoesNotReturnIf(true)] bool condition, string message)
    {
        if (!condition)
            return;

        throw new InvalidAgrumentException(message);
    }

    public static void ThrowIfNull([NotNull] object? param, string message)
    {
        if (param is null)
            throw new InvalidAgrumentException(message);
    }
}
