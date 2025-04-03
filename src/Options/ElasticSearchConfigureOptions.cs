using Infrastructure.Configuration;
using Microsoft.Extensions.Configuration;

namespace Infrastructure.Options;

/// <summary>
/// Provides configuration options for Elasticsearch connection and index settings.
/// This class validates the essential configuration required to connect to Elasticsearch.
/// </summary>
/// <remarks>
/// The class inherits from <see cref="Validator"/> to provide validation capabilities
/// for Elasticsearch configuration parameters.
/// </remarks>
public sealed class ElasticSearchConfigureOptions : Validator
{
    /// <summary>
    /// Gets or sets the connection string for Elasticsearch cluster.
    /// </summary>
    /// <value>
    /// The connection string in URI format (e.g., "http://localhost:9200").
    /// This property must be set to a non-null, non-empty value.
    /// </value>
    public string Connection { get; set; } = null!;

    /// <summary>
    /// Gets or sets the index name pattern for Elasticsearch.
    /// </summary>
    /// <value>
    /// The index name with a default pattern of "logs-YYYY" where YYYY is the current UTC year.
    /// Custom index names can be set but must not start with "logs" (case insensitive).
    /// </value>
    public string Index { get; set; } = $"logs-{DateTime.UtcNow:yyyy}";

    /// <summary>
    /// Gets or sets the configuration settings for Elasticsearch.
    /// </summary>
    /// <value>
    /// The <see cref="IConfiguration"/> instance containing additional Elasticsearch settings.
    /// This property must be set to a non-null value.
    /// </value>
    public IConfiguration Configuration { get; set; } = null!;

    /// <summary>
    /// Validates whether the Elasticsearch configuration is properly set.
    /// </summary>
    /// <returns>
    /// <c>true</c> if the configuration is valid; otherwise, <c>false</c>.
    /// </returns>
    /// <remarks>
    /// The configuration is considered valid when:
    /// <list type="bullet">
    ///   <item><description>The <see cref="Connection"/> property is not null, empty, or whitespace</description></item>
    ///   <item><description>The <see cref="Configuration"/> property is not null</description></item>
    ///   <item><description>The <see cref="Index"/> property does not start with "logs" (case insensitive)</description></item>
    /// </list>
    /// </remarks>
    public override bool IsValidConfigure()
    {
        if (string.IsNullOrWhiteSpace(Connection) || Configuration is null || Index.StartsWith("logs", StringComparison.OrdinalIgnoreCase))
            return false;

        return true;
    }
}