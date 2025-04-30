namespace Infrastructure.Exceptions.Global;

/// <summary>
/// Represents the base class for all custom exceptions in the infrastructure layer.
/// </summary>
/// <remarks>
/// This class provides a common foundation for infrastructure-related exceptions.
/// Derived classes should use this as a base to ensure consistent exception handling
/// and to provide additional context-specific details.
/// </remarks>
public abstract class InfrastructureException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="InfrastructureException"/> class.
    /// </summary>
    private InfrastructureException() : base()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="InfrastructureException"/> class with a specified error message.
    /// </summary>
    /// <param name="message">The error message that explains the reason for the exception.</param>
    public InfrastructureException(string? message) : base(message)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="InfrastructureException"/> class with a specified error message
    /// and a reference to the inner exception that is the cause of this exception.
    /// </summary>
    /// <param name="message">The error message that explains the reason for the exception.</param>
    /// <param name="innerException">The exception that is the cause of the current exception.</param>
    protected InfrastructureException(string? message, Exception? innerException) : base(message, innerException)
    {
    }
}