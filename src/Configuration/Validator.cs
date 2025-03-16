namespace Infrastructure.Configuration;

/// <summary>
/// Abstract base class for validating configuration options.
/// </summary>
public abstract class Validator
{
    /// <summary>
    /// Validates whether the configuration is correctly set up.
    /// </summary>
    /// <returns><c>true</c> if the configuration is valid; otherwise, <c>false</c>.</returns>
    public abstract bool IsValidConfigure();

    /// <summary>
    /// Gets or sets a value indicating whether the configuration is enabled.
    /// </summary>
    /// <value><c>true</c> if the configuration is enabled; otherwise, <c>false</c>.</value>
    public bool IsEnable { get; set; }
}