using Infrastructure.Data_Transfer_Object.Base;
using MimeKit;
using System.ComponentModel.DataAnnotations;

namespace Infrastructure.Data_Transfer_Object;

/// <summary>
/// Represents details required to send an email.
/// This class encapsulates information such as the recipient's username, email address, subject, and the email content.
/// </summary>
/// <remarks>
/// <para>
/// This class is used to pass email-related data to a mail service for sending emails.
/// The <see cref="Entity"/> property contains the email body, which can be a plain text, HTML, or a more complex MIME entity.
/// </para>
/// <para>
/// IMPORTANT: Ownership of the <see cref="Entity"/> resource is transferred to the recipient <see cref="MimeMessage"/>
/// when assigned to its <c>Body</c> property. Do not dispose <see cref="Entity"/> manually after this transfer.
/// </para>
/// </remarks>
public class MailDetails : MessageDetails
{
    /// <summary>
    /// Gets or sets the username of the recipient.
    /// </summary>
    [Required]
    public string UsernameTo { get; set; } = null!;

    /// <summary>
    /// Gets or sets the subject of the email.
    /// </summary>
    [Required]
    public string Subject { get; set; } = null!;

    /// <summary>
    /// Gets or sets the MIME entity representing the email body.
    /// This can include plain text, HTML, attachments, or other MIME-compliant content.
    /// </summary>
    /// <remarks>
    /// <para>
    /// When this property is assigned to <see cref="MimeMessage.Body"/>, ownership of the resource
    /// is transferred to the <see cref="MimeMessage"/> instance, which becomes responsible for its disposal.
    /// </para>
    /// <para>
    /// Do not dispose this object manually after assignment to avoid double disposal.
    /// </para>
    /// </remarks>
    [Required]
    public MimeEntity Entity { get; set; } = null!;
}
