using Infrastructure.Configuration;
using Microsoft.Extensions.Configuration;

namespace Infrastructure.Options;

/// <summary>
/// Class for configuring Elasticsearch settings.
/// </summary>
public sealed class ElasticSearchConfigureOptions : Validator
{
    /// <summary>
    /// Gets or sets the connection string for Elasticsearch.
    /// </summary>
    /// <value>The connection string for Elasticsearch.</value>
    public string Connection { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the configuration settings for Elasticsearch.
    /// </summary>
    /// <value>The configuration settings for Elasticsearch.</value>
    public IConfiguration Configuration { get; set; } = null!;

    /// <summary>
    /// Validates whether the instance is configured correctly.
    /// </summary>
    /// <returns><c>true</c> if the configuration is valid; otherwise, <c>false</c>.</returns>
    /// <remarks>
    /// The configuration is considered valid if:
    /// - The <see cref="Connection"/> property is not null or empty.
    /// - The <see cref="Configuration"/> property is not null.
    /// </remarks>
    public override bool IsValidConfigure()
    {
        if (string.IsNullOrWhiteSpace(Connection) || Configuration is null)
            return false;

        return true;
    }
}