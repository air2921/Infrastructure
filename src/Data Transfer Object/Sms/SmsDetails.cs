namespace Infrastructure.Data_Transfer_Object.Sms;

/// <summary>
/// Represents the details of an SMS message, including the recipient's phone number and the message content.
/// </summary>
/// <remarks>
/// The <see cref="SmsDetails"/> class encapsulates the essential information needed to send an SMS message:
/// - The phone number of the recipient.
/// - The content of the message to be sent.
/// </remarks>
public class SmsDetails
{
    /// <summary>
    /// Gets or sets the phone number of the recipient.
    /// </summary>
    /// <value>The phone number to which the SMS message will be sent.</value>
    /// <remarks>
    /// This value should be in a valid international format (e.g., +1XXXXXXXXXX).
    /// It is used as the destination phone number when sending an SMS.
    /// </remarks>
    public string PhoneNumber { get; set; } = null!;

    /// <summary>
    /// Gets or sets the content of the SMS message to be sent.
    /// </summary>
    /// <value>The content of the SMS message.</value>
    /// <remarks>
    /// This value represents the message body that will be sent to the recipient's phone.
    /// The message text should not be null or empty to ensure a valid SMS is sent.
    /// </remarks>
    public string Message { get; set; } = null!;
}