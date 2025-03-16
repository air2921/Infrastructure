using Infrastructure.Configuration;
using System.Text;

namespace Infrastructure.Options;

/// <summary>
/// Class for configuring authorization settings.
/// </summary>
public sealed class AuthorizationConfigureOptions : Validator
{
    /// <summary>
    /// Gets or sets the encoding used for authorization processes. Defaults to UTF-8.
    /// </summary>
    /// <value>The encoding used for authorization.</value>
    public Encoding Encoding { get; set; } = Encoding.UTF8;

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
    /// </remarks>
    public override bool IsValidConfigure()
    {
        if (string.IsNullOrWhiteSpace(Key) || string.IsNullOrWhiteSpace(Issuer) || string.IsNullOrWhiteSpace(Audience))
            return false;

        if (Expiration == TimeSpan.Zero || Expiration.TotalSeconds <= 900)
            return false;

        return true;
    }
}
