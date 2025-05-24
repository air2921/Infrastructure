using Infrastructure.Configuration;
using Microsoft.Extensions.Configuration;
using Serilog.Events;
using Serilog.Sinks.Elasticsearch;

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
    /// This property must be set to a non-null, non-empty value if <see cref="SinkOptions"/> is null.
    /// </value>
    public string Connection { internal get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets additional custom properties for Elasticsearch configuration.
    /// </summary>
    /// <value>
    /// A dictionary of key-value pairs representing custom Elasticsearch properties.
    /// These properties can be used to extend configuration beyond the standard options.
    /// </value>
    public Dictionary<string, object> Properties { internal get; set; } = [];

    /// <summary>
    /// Gets or sets the index name pattern for Elasticsearch.
    /// </summary>
    /// <value>
    /// The index name with a default pattern of "logs-YYYY" where YYYY is the current UTC year.
    /// Custom index names can be set but must start with "logs" (case insensitive).
    /// </value>
    public string Index { internal get; set; } = $"logs-{DateTime.UtcNow:yyyy.MM.dd}";

    /// <summary>
    /// Gets or sets the options for the Elasticsearch sink in Serilog.
    /// </summary>
    /// <value>
    /// The <see cref="ElasticsearchSinkOptions"/> containing specific settings for Serilog's Elasticsearch sink.
    /// If not provided, default sink options will be used based on other configuration properties.
    /// </value>
    public ElasticsearchSinkOptions? SinkOptions { internal get; set; }

    /// <summary>
    /// Gets or sets the minimum log level for Elasticsearch.
    /// </summary>
    /// <value>
    /// The minimum log event level. Defaults to <see cref="LogEventLevel.Information"/>.
    /// </value>
    public LogEventLevel EventLevel { internal get; set; } = LogEventLevel.Information;

    /// <summary>
    /// Gets or sets the configuration settings for Elasticsearch.
    /// </summary>
    /// <value>
    /// The <see cref="IConfiguration"/> instance containing additional Elasticsearch settings.
    /// This property must be set to a non-null value.
    /// </value>
    public IConfiguration Configuration { internal get; set; } = null!;

    /// <summary>
    /// Validates the Elasticsearch configuration options.
    /// </summary>
    /// <returns>
    /// <c>true</c> if the configuration is valid based on the following criteria:
    /// <list type="bullet">
    ///     <item><description><see cref="Configuration"/> is not null (required in all cases)</description></item>
    ///     <item><description>Either <see cref="SinkOptions"/> is provided (overrides other checks)</description></item>
    ///     <item><description>OR both <see cref="Connection"/> is not empty AND <see cref="Index"/> starts with "logs" (case-insensitive)</description></item>
    /// </list>
    /// Otherwise returns <c>false</c>.
    /// </returns>
    public override bool IsValidConfigure()
    {
        if (Configuration is null)
            return false;

        if (SinkOptions is not null)
            return true;

        if (!string.IsNullOrWhiteSpace(Connection) || Index.StartsWith("logs", StringComparison.OrdinalIgnoreCase))
            return true;

        return false;
    }
}