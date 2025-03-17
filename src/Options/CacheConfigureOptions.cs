using Infrastructure.Configuration;
using System.Text.Json;

namespace Infrastructure.Options;

/// <summary>
/// Class for configuring cache settings.
/// </summary>
public sealed class CacheConfigureOptions : Validator
{
    /// <summary>
    /// Internal class implementing a JSON naming policy that converts property names to lowercase.
    /// </summary>
    private class LowerCaseNamingPolicy : JsonNamingPolicy
    {
        /// <summary>
        /// Converts a property name to lowercase.
        /// </summary>
        /// <param name="name">The property name to convert.</param>
        /// <returns>The property name in lowercase.</returns>
        public override string ConvertName(string name)
            => name.ToLowerInvariant();
    }

    private JsonNamingPolicy _jsonNamingPolicy;
    private JsonSerializerOptions _jsonSerializerSettings;

    /// <summary>
    /// Initializes a new instance of the <see cref="CacheConfigureOptions"/> class.
    /// </summary>
    public CacheConfigureOptions()
    {
        _jsonNamingPolicy = new LowerCaseNamingPolicy();
        _jsonSerializerSettings = new JsonSerializerOptions
        {
            Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
            PropertyNamingPolicy = _jsonNamingPolicy,
            WriteIndented = true
        };
    }

    /// <summary>
    /// Gets or sets the JSON serializer settings.
    /// </summary>
    /// <value>The JSON serializer settings.</value>
    public JsonSerializerOptions JsonSerializerSettings
    {
        get => _jsonSerializerSettings;
        set => _jsonSerializerSettings = value;
    }

    /// <summary>
    /// Gets or sets the JSON naming policy.
    /// </summary>
    /// <value>The JSON naming policy.</value>
    public JsonNamingPolicy JsonNamingPolicy
    {
        get => _jsonNamingPolicy;
        set => _jsonNamingPolicy = value;
    }

    /// <summary>
    /// Gets or sets the connection string.
    /// </summary>
    /// <value>The connection string.</value>
    public string Connection { get; set; } = null!;

    /// <summary>
    /// Validates whether the instance is configured correctly.
    /// </summary>
    /// <returns><c>true</c> if the configuration is valid; otherwise, <c>false</c>.</returns>
    public override bool IsValidConfigure()
    {
        if (string.IsNullOrWhiteSpace(Connection))
            return false;

        return true;
    }
}
