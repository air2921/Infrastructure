namespace Infrastructure.Enums;

/// <summary>
/// Enumeration of ports for SMTP connections.
/// </summary>
public enum SmtpPort
{
    /// <summary>
    /// Standard SMTP port (25).
    /// Used for mail transfer between mail servers.
    /// </summary>
    Smtp = 25,

    /// <summary>
    /// SMTP port with SSL (465).
    /// Used for secure connections with the mail server.
    /// </summary>
    Smtps = 465,

    /// <summary>
    /// SMTP submission port with authentication via TLS (587).
    /// Commonly used for sending mail with authentication.
    /// </summary>
    SmtpSub = 587,

    /// <summary>
    /// Alternative SMTP port (2525).
    /// Used when port 25 is blocked on a server or network.
    /// </summary>
    SmtpAlt = 2525
}