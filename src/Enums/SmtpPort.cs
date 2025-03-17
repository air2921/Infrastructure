namespace Infrastructure.Enums;

/// <summary>
/// Enumeration of ports for SMTP connections.
/// <b>Port 587 is recommended for sending mail with authentication over TLS.</b>
/// </summary>
public enum SmtpPort
{
    /// <summary>
    /// Standard SMTP port (25).
    /// Used for mail transfer between mail servers.
    /// Typically not recommended for sending mail to avoid issues with spam filters.
    /// </summary>
    Smtp = 25,

    [Obsolete("Port 465 is deprecated and should be avoided. Use port 587 instead.")]
    /// <summary>
    /// SMTP port with SSL (465).
    /// Used for secure connections with the mail server.
    /// This port is deprecated and should be avoided in favor of port 587.
    /// </summary>
    Smtps = 465,

    /// <summary>
    /// SMTP submission port with authentication via TLS (587).
    /// <b>This is the recommended port for sending mail with authentication.</b>
    /// Commonly used for sending mail securely with authentication and encryption.
    /// </summary>
    SmtpSub = 587,

    /// <summary>
    /// Alternative SMTP port (2525).
    /// Used when port 25 is blocked on a server or network.
    /// Can be used as a fallback, but not the preferred choice.
    /// </summary>
    SmtpAlt = 2525
}