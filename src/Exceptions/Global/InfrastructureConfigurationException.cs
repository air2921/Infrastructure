using System.Diagnostics.CodeAnalysis;

namespace Infrastructure.Exceptions.Global;

/// <summary>
/// The exception that is thrown when an error occurs during the configuration of infrastructure services.
/// </summary>
/// <remarks>
/// This exception is typically thrown when invalid or missing configuration data is detected
/// during the setup of infrastructure components.
/// </remarks>
public sealed class InfrastructureConfigurationException : ArgumentException
{
    /// <summary>
    /// Initializes a new instance of the <see cref="InfrastructureConfigurationException"/> class.
    /// </summary>
    private InfrastructureConfigurationException() : base()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="InfrastructureConfigurationException"/> class with a specified error message.
    /// </summary>
    /// <param name="message">The error message that explains the reason for the exception.</param>
    public InfrastructureConfigurationException(string? message) : base(message)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="InfrastructureConfigurationException"/> class with a specified error message
    /// and the name of the parameter that caused the exception.
    /// </summary>
    /// <param name="message">The error message that explains the reason for the exception.</param>
    /// <param name="argument">The name of the parameter that caused the exception.</param>
    public InfrastructureConfigurationException(string? message, string? argument) : base(message, argument)
    {
    }

    /// <summary>
    /// Throws an <see cref="InfrastructureConfigurationException"/> if the specified condition is <c>true</c>.
    /// This method helps to conditionally throw the exception based on the provided condition.
    /// </summary>
    /// <param name="condition">The condition to evaluate. If <c>true</c>, an exception is thrown.</param>
    /// <param name="message">The error message that explains the reason for the exception.</param>
    /// <param name="instance">
    /// An optional instance of the object that caused the exception. 
    /// If provided, the name of the instance's type will be included in the exception details.
    /// </param>
    /// <exception cref="InfrastructureConfigurationException">
    /// Thrown when <paramref name="condition"/> is <c>true</c>.
    /// </exception>
    /// <remarks>
    /// This method is useful for validating conditions and throwing exceptions with additional context,
    /// such as the instance of the object that caused the validation to fail.
    /// </remarks>
    public static void ThrowIf([DoesNotReturnIf(true)] bool condition, string message, object? instance = null)
    {
        if (!condition)
            return;

        throw new InfrastructureConfigurationException(message, instance is null ? null : nameof(instance));
    }
}
