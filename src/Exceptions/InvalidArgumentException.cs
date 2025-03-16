using Infrastructure.Exceptions.Global;
using System.Diagnostics.CodeAnalysis;

namespace Infrastructure.Exceptions;

/// <summary>
/// Represents an exception that is thrown when an invalid argument is provided.
/// </summary>
/// <remarks>
/// This exception is used to handle errors where a method or constructor receives an argument
/// that is invalid or does not meet the expected criteria. It provides utility methods for
/// conditional exception throwing.
/// </remarks>
public class InvalidArgumentException : InfrastructureException
{
    /// <summary>
    /// Initializes a new instance of the <see cref="InvalidArgumentException"/> class.
    /// </summary>
    public InvalidArgumentException() : base()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="InvalidArgumentException"/> class with a specified error message.
    /// </summary>
    /// <param name="message">The error message that explains the reason for the exception.</param>
    public InvalidArgumentException(string? message) : base(message)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="InvalidArgumentException"/> class with a specified error message
    /// and a reference to the inner exception that is the cause of this exception.
    /// </summary>
    /// <param name="message">The error message that explains the reason for the exception.</param>
    /// <param name="exception">The exception that is the cause of the current exception, or <c>null</c> if no inner exception is specified.</param>
    public InvalidArgumentException(string? message, Exception? exception) : base(message, exception)
    {
    }

    /// <summary>
    /// Throws an <see cref="InvalidArgumentException"/> if the specified condition is <c>true</c>.
    /// </summary>
    /// <param name="condition">The condition to evaluate. If <c>true</c>, an exception is thrown.</param>
    /// <param name="message">The error message that explains the reason for the exception.</param>
    /// <exception cref="InvalidArgumentException">Thrown when <paramref name="condition"/> is <c>true</c>.</exception>
    public static void ThrowIf([DoesNotReturnIf(true)] bool condition, string message)
    {
        if (!condition)
            return;

        throw new InvalidArgumentException(message);
    }

    /// <summary>
    /// Throws an <see cref="InvalidArgumentException"/> if the specified parameter is <c>null</c>.
    /// </summary>
    /// <param name="param">The parameter to check for <c>null</c>.</param>
    /// <param name="message">The error message that explains the reason for the exception.</param>
    /// <exception cref="InvalidArgumentException">Thrown when <paramref name="param"/> is <c>null</c>.</exception>
    public static void ThrowIfNull([NotNull] object? param, string message)
    {
        if (param is null)
            throw new InvalidArgumentException(message);
    }
}
