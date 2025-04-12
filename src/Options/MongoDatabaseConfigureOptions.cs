using Infrastructure.Configuration;

namespace Infrastructure.Options;

/// <summary>
/// Class for configuring MongoDB settings.
/// </summary>
public sealed class MongoDatabaseConfigureOptions : Validator
{
    /// <summary>
    /// Gets or sets the connection string for the MongoDB database.
    /// </summary>
    /// <value>The connection string for the MongoDB database.</value>
    public string Connection { internal get; set; } = null!;

    /// <summary>
    /// Gets or sets the name of the MongoDB database.
    /// </summary>
    /// <value>The name of the MongoDB database.</value>
    public string Database { internal get; set; } = null!;

    /// <summary>
    /// Validates whether the instance is configured correctly.
    /// </summary>
    /// <returns><c>true</c> if the configuration is valid; otherwise, <c>false</c>.</returns>
    /// <remarks>
    /// The configuration is considered valid if:
    /// - The <see cref="Connection"/> property is not null or empty.
    /// - The <see cref="Database"/> property is not null or empty.
    /// </remarks>
    public override bool IsValidConfigure()
    {
        if (string.IsNullOrWhiteSpace(Connection) || string.IsNullOrWhiteSpace(Database))
            return false;

        return true;
    }
}
