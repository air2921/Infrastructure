using Infrastructure.Exceptions.Global;

namespace Infrastructure.Configuration;

/// <summary>
/// Abstract base class for validating configuration options.
/// </summary>
/// <remarks>
/// This class provides a foundation for validating configuration settings in derived classes.
/// It includes methods to check if the configuration is valid and to ensure successful validation.
/// </remarks>
public abstract class Validator
{
    /// <summary>
    /// Validates whether the configuration is correctly set up.
    /// </summary>
    /// <returns>
    /// <c>true</c> if the configuration is valid; otherwise, <c>false</c>.
    /// </returns>
    /// <remarks>
    /// This method must be implemented by derived classes to provide specific validation logic
    /// for the configuration options.
    /// </remarks>
    public abstract bool IsValidConfigure();

    /// <summary>
    /// Ensures that the configuration is valid. If the configuration is invalid, an exception is thrown.
    /// </summary>
    /// <remarks>
    /// This method calls the <see cref="IsValidConfigure"/> method to check if the configuration is valid.
    /// If the configuration is invalid, it throws an <see cref="InfrastructureConfigurationException"/>
    /// with the message "Invalid options configuration".
    /// </remarks>
    public virtual void EnsureSuccessValidation(string? message = null)
    {
        var isValid = IsValidConfigure();
        InfrastructureConfigurationException.ThrowIf(isValid == false, message ?? "Invalid configuration options", this);
    }

    /// <summary>
    /// Gets or sets a value indicating whether the configuration is enabled.
    /// </summary>
    /// <value>
    /// <c>true</c> if the configuration is enabled; otherwise, <c>false</c>.
    /// </value>
    /// <remarks>
    /// This property is used to dynamically enable or disable the associated feature or configuration.
    /// When set to <c>false</c>, the feature or configuration will be bypassed or ignored, allowing for conditional
    /// execution based on runtime requirements. This is particularly useful for feature toggles or when certain
    /// configurations should only be applied under specific conditions.
    /// </remarks>
    public bool IsEnable { get; set; }
}