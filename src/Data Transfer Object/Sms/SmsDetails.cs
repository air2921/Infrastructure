using Infrastructure.Data_Transfer_Object.Base;

namespace Infrastructure.Data_Transfer_Object.Sms;

/// <summary>
/// Represents the details of an SMS message, including the recipient's phone number and the message content.
/// </summary>
/// <remarks>
/// The <see cref="SmsDetails"/> class encapsulates the essential information needed to send an SMS message:
/// - The phone number of the recipient
/// - The content of the message to be sent
/// - The optional sender phone number
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
    /// Maximum length is 1600 characters according to Twilio's limitations.
    /// </remarks>
    public required string Message { get; set; }

    /// <summary>
    /// Gets or sets the sender's phone number for the SMS message.
    /// </summary>
    /// <value>
    /// The Twilio phone number from which the SMS will be sent (in E.164 format, e.g. "+15551234567").
    /// If null, the default or a randomly selected registered number will be used.
    /// </value>
    /// <remarks>
    /// This property is optional. When not specified, the SMS sending service should:
    /// 1. Use a default configured phone number, or
    /// 2. Select a random number from the pool of registered numbers
    /// The specified number must be verified/registered in your Twilio account.
    /// </remarks>
    public string? From { get; set; }
}