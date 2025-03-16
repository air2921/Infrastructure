using Infrastructure.Exceptions.Global;
using System.Diagnostics.CodeAnalysis;

namespace Infrastructure.Exceptions;

/// <summary>
/// Represents an exception that is thrown when an error occurs in distributed cache operations.
/// </summary>
/// <remarks>
/// This exception is used to handle errors specific to distributed caching, such as connection failures,
/// serialization issues, or cache key conflicts. It provides utility methods for conditional exception throwing.
/// </remarks>
public class DistributedCacheException : InfrastructureException
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DistributedCacheException"/> class.
    /// </summary>
    public DistributedCacheException() : base()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="DistributedCacheException"/> class with a specified error message.
    /// </summary>
    /// <param name="message">The error message that explains the reason for the exception.</param>
    public DistributedCacheException(string? message) : base(message)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="DistributedCacheException"/> class with a specified error message
    /// and a reference to the inner exception that is the cause of this exception.
    /// </summary>
    /// <param name="message">The error message that explains the reason for the exception.</param>
    /// <param name="exception">The exception that is the cause of the current exception, or <c>null</c> if no inner exception is specified.</param>
    public DistributedCacheException(string? message, Exception? exception) : base(message, exception)
    {
    }

    /// <summary>
    /// Throws a <see cref="DistributedCacheException"/> if the specified condition is <c>true</c>.
    /// </summary>
    /// <param name="condition">The condition to evaluate. If <c>true</c>, an exception is thrown.</param>
    /// <param name="message">The error message that explains the reason for the exception.</param>
    /// <exception cref="DistributedCacheException">Thrown when <paramref name="condition"/> is <c>true</c>.</exception>
    public static void ThrowIf([DoesNotReturnIf(true)] bool condition, string message)
    {
        if (!condition)
            return;

        throw new DistributedCacheException(message);
    }

    /// <summary>
    /// Throws a <see cref="DistributedCacheException"/> if the specified parameter is <c>null</c>.
    /// </summary>
    /// <param name="param">The parameter to check for <c>null</c>.</param>
    /// <param name="message">The error message that explains the reason for the exception.</param>
    /// <exception cref="DistributedCacheException">Thrown when <paramref name="param"/> is <c>null</c>.</exception>
    public static void ThrowIfNull([NotNull] object? param, string message)
    {
        if (param is null)
            throw new DistributedCacheException(message);
    }
}
