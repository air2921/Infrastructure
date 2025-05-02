using Infrastructure.Configuration;

namespace Infrastructure.Options;

/// <summary>
/// Class for configuring Entity Framework settings.
/// </summary>
public sealed class EntityFrameworkConfigureOptions : Validator
{
    /// <summary>
    /// Validates whether the instance is configured correctly.
    /// </summary>
    /// <returns><c>true</c> if the configuration is valid; otherwise, <c>false</c>.</returns>
    public override bool IsValidConfigure()
        => true;
}