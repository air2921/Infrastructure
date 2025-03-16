using Infrastructure.Exceptions.Global;
using System.Diagnostics.CodeAnalysis;

namespace Infrastructure.Exceptions;

/// <summary>
/// Represents an exception that is thrown when an error occurs during database operations.
/// </summary>
/// <remarks>
/// This exception is used to handle errors specific to database interactions, such as SQL queries,
/// MongoDB operations, or other database-related issues. It provides utility methods for conditional exception throwing.
/// </remarks>
public class EntityException : InfrastructureException
{
    /// <summary>
    /// Initializes a new instance of the <see cref="EntityException"/> class.
    /// </summary>
    public EntityException() : base()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="EntityException"/> class with a specified error message.
    /// </summary>
    /// <param name="message">The error message that explains the reason for the exception.</param>
    public EntityException(string? message) : base(message)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="EntityException"/> class with a specified error message
    /// and a reference to the inner exception that is the cause of this exception.
    /// </summary>
    /// <param name="message">The error message that explains the reason for the exception.</param>
    /// <param name="exception">The exception that is the cause of the current exception, or <c>null</c> if no inner exception is specified.</param>
    public EntityException(string? message, Exception? exception) : base(message, exception)
    {
    }

    /// <summary>
    /// Throws an <see cref="EntityException"/> if the specified condition is <c>true</c>.
    /// </summary>
    /// <param name="condition">The condition to evaluate. If <c>true</c>, an exception is thrown.</param>
    /// <param name="message">The error message that explains the reason for the exception.</param>
    /// <exception cref="EntityException">Thrown when <paramref name="condition"/> is <c>true</c>.</exception>
    public static void ThrowIf([DoesNotReturnIf(true)] bool condition, string message)
    {
        if (!condition)
            return;

        throw new EntityException(message);
    }

    /// <summary>
    /// Throws an <see cref="EntityException"/> if the specified parameter is <c>null</c>.
    /// </summary>
    /// <param name="param">The parameter to check for <c>null</c>.</param>
    /// <param name="message">The error message that explains the reason for the exception.</param>
    /// <exception cref="EntityException">Thrown when <paramref name="param"/> is <c>null</c>.</exception>
    public static void ThrowIfNull([NotNull] object? param, string message)
    {
        if (param is null)
            throw new EntityException(message);
    }
}
