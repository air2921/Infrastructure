using Infrastructure.Configuration;
using System.Text.Json;

namespace Infrastructure.Options;

public class CacheConfigureOptions : Validator
{
    private class LowerCaseNamingPolicy : JsonNamingPolicy
    {
        public override string ConvertName(string name)
            => name.ToLowerInvariant();
    }

    private JsonNamingPolicy _jsonNamingPolicy;
    private JsonSerializerOptions _jsonSerializerSettings;

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

    public JsonSerializerOptions JsonSerializerSettings
    {
        get => _jsonSerializerSettings;
        set => _jsonSerializerSettings = value;
    }

    public JsonNamingPolicy JsonNamingPolicy
    {
        get => _jsonNamingPolicy;
        set => _jsonNamingPolicy = value;
    }

    public string Connection { get; set; } = string.Empty;

    public override bool IsValidConfigure()
    {
        if (string.IsNullOrWhiteSpace(Connection))
            return false;

        return true;
    }
}
