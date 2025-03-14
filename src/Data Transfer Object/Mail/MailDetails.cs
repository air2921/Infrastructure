﻿using MimeKit;

namespace Infrastructure.Data_Transfer_Object;

/// <summary>
/// Represents details required to send an email.
/// This class encapsulates information such as the recipient's username, email address, subject, and the email content.
/// </summary>
/// <remarks>
/// This class is used to pass email-related data to a mail service for sending emails.
/// The <see cref="Entity"/> property contains the email body, which can be a plain text, HTML, or a more complex MIME entity.
/// </remarks>
public class MailDetails
{
    /// <summary>
    /// Gets or sets the username of the recipient.
    /// </summary>
    /// <example>
    /// <code>
    /// var mailDetails = new MailDetails
    /// {
    ///     UsernameTo = "JohnDoe",
    ///     EmailTo = "john.doe@example.com",
    ///     Subject = "Welcome to Our Service",
    ///     Entity = new TextPart("plain") { Text = "Hello, John!" }
    /// };
    /// </code>
    /// </example>
    public required string UsernameTo { get; set; }

    /// <summary>
    /// Gets or sets the email address of the recipient.
    /// </summary>
    /// <example>
    /// <code>
    /// var mailDetails = new MailDetails
    /// {
    ///     UsernameTo = "JohnDoe",
    ///     EmailTo = "john.doe@example.com",
    ///     Subject = "Welcome to Our Service",
    ///     Entity = new TextPart("plain") { Text = "Hello, John!" }
    /// };
    /// </code>
    /// </example>
    public required string EmailTo { get; set; }

    /// <summary>
    /// Gets or sets the subject of the email.
    /// </summary>
    /// <example>
    /// <code>
    /// var mailDetails = new MailDetails
    /// {
    ///     UsernameTo = "JohnDoe",
    ///     EmailTo = "john.doe@example.com",
    ///     Subject = "Welcome to Our Service",
    ///     Entity = new TextPart("plain") { Text = "Hello, John!" }
    /// };
    /// </code>
    /// </example>
    public required string Subject { get; set; }

    /// <summary>
    /// Gets or sets the MIME entity representing the email body.
    /// This can include plain text, HTML, attachments, or other MIME-compliant content.
    /// </summary>
    /// <example>
    /// <code>
    /// var mailDetails = new MailDetails
    /// {
    ///     UsernameTo = "JohnDoe",
    ///     EmailTo = "john.doe@example.com",
    ///     Subject = "Welcome to Our Service",
    ///     Entity = new TextPart("html") { Text = "<h1>Hello, John!</h1>" }
    /// };
    /// </code>
    /// </example>
    public required MimeEntity Entity { get; set; }
}
