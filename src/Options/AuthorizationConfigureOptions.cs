using Infrastructure.Configuration;
using Infrastructure.Exceptions.Global;
using Microsoft.IdentityModel.Tokens;
using System.Reflection;
using System.Text;

namespace Infrastructure.Options;

/// <summary>
/// Configures settings for authorization processes, including token encoding, algorithm selection,
/// expiration time, signing key, issuer, and audience.
/// </summary>
/// <remarks>
/// This class provides a centralized way to configure and validate settings required for authorization,
/// such as token generation and validation. It ensures that the provided configuration is valid and
/// adheres to security best practices.
/// </remarks>
public sealed class AuthorizationConfigureOptions : Validator
{
    private static readonly IEnumerable<string> _algs;

    /// <summary>
    /// Initializes static members of the <see cref="AuthorizationConfigureOptions"/> class.
    /// </summary>
    /// <remarks>
    /// During static initialization, the constructor retrieves all available algorithms from the
    /// <see cref="SecurityAlgorithms"/> class using reflection. These algorithms are used to validate
    /// the <see cref="Algorithm"/> property during configuration validation.
    /// </remarks>
    static AuthorizationConfigureOptions()
    {
        var fields = typeof(SecurityAlgorithms).GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)
                                           .Where(f => f.IsLiteral && !f.IsInitOnly);

        _algs = fields.Select(f => f.GetRawConstantValue()).Cast<string>()
                      .Where(value => !string.IsNullOrWhiteSpace(value));
    }

    /// <summary>
    /// Gets or sets the encoding used for authorization processes. Defaults to <see cref="Encoding.UTF8"/>.
    /// </summary>
    /// <value>
    /// The encoding used for authorization token generation and validation.
    /// </value>
    public Encoding Encoding { get; set; } = Encoding.UTF8;

    /// <summary>
    /// Gets or sets the algorithm used for authorization token signing and validation.
    /// </summary>
    /// <value>
    /// The algorithm used for authorization. Must be one of the supported algorithms from <see cref="SecurityAlgorithms"/>.
    /// </value>
    public string Algorithm { get; set; } = null!;

    /// <summary>
    /// Gets or sets the expiration time for authorization tokens. Defaults to 30 minutes.
    /// </summary>
    /// <value>
    /// The expiration time for authorization tokens. Must be greater than 15 minutes (900 seconds).
    /// </value>
    public TimeSpan Expiration { get; set; } = TimeSpan.FromMinutes(30);

    /// <summary>
    /// Gets or sets the key used for signing authorization tokens.
    /// </summary>
    /// <value>
    /// The signing key used for token generation and validation. Must not be null or empty.
    /// </value>
    public string Key { get; set; } = null!;

    /// <summary>
    /// Gets or sets the issuer of the authorization tokens.
    /// </summary>
    /// <value>
    /// The issuer of the authorization tokens. Must not be null or empty.
    /// </value>
    public string Issuer { get; set; } = null!;

    /// <summary>
    /// Gets or sets the audience of the authorization tokens.
    /// </summary>
    /// <value>
    /// The audience of the authorization tokens. Must not be null or empty.
    /// </value>
    public string Audience { get; set; } = null!;

    /// <summary>
    /// Validates whether the current configuration is valid.
    /// </summary>
    /// <returns>
    /// <c>true</c> if the configuration is valid; otherwise, <c>false</c>.
    /// </returns>
    /// <remarks>
    /// The configuration is considered valid if:
    /// <list type="bullet">
    ///   <item><description>The <see cref="Key"/>, <see cref="Issuer"/>, and <see cref="Audience"/> properties are not null or empty.</description></item>
    ///   <item><description>The <see cref="Expiration"/> is greater than 15 minutes (900 seconds).</description></item>
    ///   <item><description>The <see cref="Algorithm"/> is one of the supported algorithms from <see cref="SecurityAlgorithms"/>.</description></item>
    /// </list>
    /// </remarks>
    public override bool IsValidConfigure()
    {
        if (string.IsNullOrWhiteSpace(Key) || string.IsNullOrWhiteSpace(Issuer) || string.IsNullOrWhiteSpace(Audience))
            return false;

        if (Expiration == TimeSpan.Zero || Expiration < TimeSpan.FromMinutes(30))
            return false;

        if (string.IsNullOrWhiteSpace(Algorithm) || !_algs.Contains(Algorithm))
            return false;

        return true;
    }
}
