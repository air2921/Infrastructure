using Infrastructure.Configuration;
using Infrastructure.Exceptions.Global;
using Microsoft.IdentityModel.Tokens;
using System.Reflection;
using System.Text;

namespace Infrastructure.Options;

/// <summary>
/// Class for configuring authorization settings.
/// </summary>
/// <remarks>
/// This class provides configuration options for authorization processes, including token encoding,
/// algorithm selection, expiration time, signing key, issuer, and audience.
/// </remarks>
public sealed class AuthorizationConfigureOptions : Validator
{
    private readonly IEnumerable<string> _algs;

    /// <summary>
    /// Initializes a new instance of the <see cref="AuthorizationConfigureOptions"/> class.
    /// </summary>
    /// <remarks>
    /// During initialization, the constructor retrieves all available algorithms from the
    /// <see cref="SecurityAlgorithms"/> class using reflection.
    /// </remarks>
    public AuthorizationConfigureOptions()
    {
        var fields = typeof(SecurityAlgorithms).GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)
                                           .Where(f => f.IsLiteral && !f.IsInitOnly);

        _algs = fields.Select(f => f.GetRawConstantValue()).Cast<string>()
                      .Where(value => !string.IsNullOrWhiteSpace(value));
    }

    /// <summary>
    /// Gets or sets the encoding used for authorization processes. Defaults to UTF-8.
    /// </summary>
    /// <value>The encoding used for authorization.</value>
    public Encoding Encoding { get; set; } = Encoding.UTF8;

    /// <summary>
    /// Gets or sets the algorithm used for authorization processes.
    /// </summary>
    /// <value>The algorithm used for authorization.</value>
    public string Algorithm { get; set; } = null!;

    /// <summary>
    /// Gets or sets the expiration time for authorization tokens. Defaults to 30 minutes.
    /// </summary>
    /// <value>The expiration time for authorization tokens.</value>
    public TimeSpan Expiration { get; set; } = TimeSpan.FromMinutes(30);

    /// <summary>
    /// Gets or sets the key used for signing authorization tokens.
    /// </summary>
    /// <value>The key used for signing authorization tokens.</value>
    public string Key { get; set; } = null!;

    /// <summary>
    /// Gets or sets the issuer of the authorization tokens.
    /// </summary>
    /// <value>The issuer of the authorization tokens.</value>
    public string Issuer { get; set; } = null!;

    /// <summary>
    /// Gets or sets the audience of the authorization tokens.
    /// </summary>
    /// <value>The audience of the authorization tokens.</value>
    public string Audience { get; set; } = null!;

    /// <summary>
    /// Validates whether the instance is configured correctly.
    /// </summary>
    /// <returns><c>true</c> if the configuration is valid; otherwise, <c>false</c>.</returns>
    /// <remarks>
    /// The configuration is considered valid if:
    /// - The <see cref="Key"/>, <see cref="Issuer"/>, and <see cref="Audience"/> properties are not null or empty.
    /// - The <see cref="Expiration"/> is greater than 15 minutes (900 seconds).
    /// - The <see cref="Algorithm"/> is one of the supported algorithms.
    /// </remarks>
    public override bool IsValidConfigure()
    {
        if (string.IsNullOrWhiteSpace(Key) || string.IsNullOrWhiteSpace(Issuer) || string.IsNullOrWhiteSpace(Audience))
            return false;

        if (Expiration == TimeSpan.Zero || Expiration.TotalSeconds <= 900)
            return false;

        if (string.IsNullOrWhiteSpace(Algorithm) || !_algs.Contains(Algorithm))
            return false;

        return true;
    }
}
