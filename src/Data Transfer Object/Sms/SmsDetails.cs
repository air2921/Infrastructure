using Infrastructure.Data_Transfer_Object.Base;
using System.ComponentModel.DataAnnotations;

namespace Infrastructure.Data_Transfer_Object.Sms;

/// <summary>
/// Represents the details of an SMS message, including the recipient's phone number and the message content.
/// </summary>
/// <remarks>
/// The <see cref="SmsDetails"/> class encapsulates the essential information needed to send an SMS message:
/// - The phone number of the recipient.
/// - The content of the message to be sent.
/// </remarks>
public class SmsDetails : MessageDetails
{
    /// <summary>
    /// Gets or sets the content of the SMS message to be sent.
    /// </summary>
    /// <value>The content of the SMS message.</value>
    /// <remarks>
    /// This value represents the message body that will be sent to the recipient's phone.
    /// The message text should not be null or empty to ensure a valid SMS is sent.
    /// </remarks>
    [Required]
    public string Message { get; set; } = null!;
}