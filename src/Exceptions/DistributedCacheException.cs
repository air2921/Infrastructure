using Infrastructure.Exceptions.Global;
using System.Diagnostics.CodeAnalysis;

namespace Infrastructure.Exceptions;

/// <summary>
/// Represents an exception that is thrown when an error occurs in distributed cache operations.
/// </summary>
/// <remarks>
/// This exception is used to handle errors specific to distributed caching, such as connection failures,
/// serialization issues, or cache key conflicts. It provides utility methods for conditional exception throwing,
/// ensuring that exceptions are thrown only when necessary and preventing redundant exception creation.
/// </remarks>
/// <remarks>
/// Initializes a new instance of the <see cref="DistributedCacheException"/> class with a specified error message.
/// </remarks>
/// <param name="message">The error message that explains the reason for the exception.</param>
public class DistributedCacheException(string? message) : InfrastructureException(message)
{
    private static readonly HashSet<DistributedCacheException> _errors = [];

    /// <summary>
    /// Throws a <see cref="DistributedCacheException"/> if the specified condition is <c>true</c>.
    /// This method helps to conditionally throw the exception based on the provided condition.
    /// If an exception with the same message has already been thrown, it will be reused.
    /// </summary>
    /// <param name="condition">The condition to evaluate. If <c>true</c>, an exception is thrown.</param>
    /// <param name="message">The error message that explains the reason for the exception.</param>
    /// <exception cref="DistributedCacheException">Thrown when <paramref name="condition"/> is <c>true</c>.</exception>
    public static void ThrowIf([DoesNotReturnIf(true)] bool condition, string message)
    {
        if (!condition)
            return;

        var error = _errors.FirstOrDefault(x => x.Message == message);
        if (error is not null)
            throw error;

        error = new DistributedCacheException(message);
        _errors.Add(error);

        throw error;
    }

    /// <summary>
    /// Throws a <see cref="DistributedCacheException"/> if the specified parameter is <c>null</c>.
    /// This method allows you to throw an exception when an important parameter is found to be <c>null</c>.
    /// If an exception with the same message has already been thrown, it will be reused.
    /// </summary>
    /// <param name="param">The parameter to check for <c>null</c>.</param>
    /// <param name="message">The error message that explains the reason for the exception.</param>
    /// <exception cref="DistributedCacheException">Thrown when <paramref name="param"/> is <c>null</c>.</exception>
    public static void ThrowIfNull([NotNull] object? param, string message)
    {
        if (param is not null)
            return;

        var error = _errors.FirstOrDefault(x => x.Message == message);
        if (error is not null)
            throw error;

        error = new DistributedCacheException(message);
        _errors.Add(error);

        throw error;
    }

    /// <summary>
    /// Compares the current <see cref="DistributedCacheException"/> object with another object.
    /// Two <see cref="DistributedCacheException"/> objects are considered equal if they have the same message.
    /// </summary>
    /// <param name="obj">The object to compare with the current exception.</param>
    /// <returns><c>true</c> if the current exception is equal to the specified object; otherwise, <c>false</c>.</returns>
    public override bool Equals(object? obj)
    {
        if (obj is not DistributedCacheException other)
            return false;

        if (Message != other.Message)
            return false;

        return true;
    }

    /// <summary>
    /// Gets the hash code for the current <see cref="DistributedCacheException"/> object.
    /// The hash code is based on the exception's message.
    /// </summary>
    /// <returns>The hash code for the current <see cref="DistributedCacheException"/>.</returns>
    public override int GetHashCode()
        => HashCode.Combine(Message);
}
