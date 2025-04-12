using Infrastructure.Configuration;
using Infrastructure.Enums;

namespace Infrastructure.Options;

/// <summary>
/// Class for configuring Entity Framework settings.
/// </summary>
public sealed class EntityFrameworkConfigureOptions : Validator
{
    /// <summary>
    /// Gets or sets the connection string for the database.
    /// </summary>
    /// <value>The connection string for the database.</value>
    public string Connection { internal get; set; } = null!;

    /// <summary>
    /// Gets or sets the SQL database configuration.
    /// </summary>
    /// <value>The SQL database configuration.</value>
    public SqlDatabase Database { internal get; set; }

    /// <summary>
    /// Validates whether the instance is configured correctly.
    /// </summary>
    /// <returns><c>true</c> if the configuration is valid; otherwise, <c>false</c>.</returns>
    /// <remarks>
    /// The configuration is considered valid if:
    /// - The <see cref="Connection"/> property is not null or empty.
    /// </remarks>
    public override bool IsValidConfigure()
    {
        if (string.IsNullOrWhiteSpace(Connection))
            return false;

        return true;
    }
}