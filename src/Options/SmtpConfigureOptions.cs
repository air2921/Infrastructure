using Infrastructure.Configuration;

namespace Infrastructure.Options;

/// <summary>
/// Class for configuring SMTP (Simple Mail Transfer Protocol) settings.
/// </summary>
public sealed class SmtpConfigureOptions : Validator
{
    /// <summary>
    /// Gets or sets the SMTP provider (e.g., Gmail, Outlook, etc.).
    /// </summary>
    /// <value>The SMTP provider.</value>
    public string Provider { get; set; } = null!;

    /// <summary>
    /// Gets or sets the sender's display name for emails.
    /// </summary>
    /// <value>The sender's display name.</value>
    public string SenderName { get; set; } = null!;

    /// <summary>
    /// Gets or sets the email address used for sending emails.
    /// </summary>
    /// <value>The email address.</value>
    public string Address { get; set; } = null!;

    /// <summary>
    /// Gets or sets the password for the email account.
    /// </summary>
    /// <value>The email account password.</value>
    public string Password { get; set; } = null!;

    /// <summary>
    /// Gets or sets the port number for the SMTP server.
    /// </summary>
    /// <value>The SMTP server port.</value>
    public int Port { get; set; }

    /// <summary>
    /// Validates whether the instance is configured correctly.
    /// </summary>
    /// <returns><c>true</c> if the configuration is valid; otherwise, <c>false</c>.</returns>
    /// <remarks>
    /// The configuration is considered valid if:
    /// - The <see cref="Provider"/> property is not null or empty.
    /// - The <see cref="Address"/> property is not null or empty.
    /// - The <see cref="Password"/> property is not null or empty.
    /// - The <see cref="SenderName"/> property is not null or empty.
    /// - The <see cref="Port"/> property is a valid SMTP port (25, 2525, 465, or 587).
    /// </remarks>
    public override bool IsValidConfigure()
    {
        if (string.IsNullOrWhiteSpace(Provider) || string.IsNullOrWhiteSpace(Address) ||
            string.IsNullOrWhiteSpace(Password) || string.IsNullOrWhiteSpace(SenderName) || !IsValidPort(Port))
            return false;

        return true;
    }

    /// <summary>
    /// Validates whether the specified port is a valid SMTP port.
    /// </summary>
    /// <param name="port">The port number to validate.</param>
    /// <returns><c>true</c> if the port is valid; otherwise, <c>false</c>.</returns>
    /// <remarks>
    /// Valid SMTP ports are: 25, 2525, 465, and 587.
    /// </remarks>
    private bool IsValidPort(int port)
    {
        int[] ports = [25, 2525, 465, 587];

        if (!ports.Contains(port))
            return false;

        return true;
    }
}
