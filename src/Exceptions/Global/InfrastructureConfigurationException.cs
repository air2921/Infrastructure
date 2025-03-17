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
}
