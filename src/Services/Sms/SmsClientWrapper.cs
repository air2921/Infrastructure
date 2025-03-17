using Infrastructure.Options;
using Twilio;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Types;

namespace Infrastructure.Services.Sms;

/// <summary>
/// A wrapper class for the Twilio SMS client that provides a simplified API for sending SMS messages through Twilio.
/// This class handles the initialization of the Twilio client and provides methods for sending messages synchronously and asynchronously.
/// </summary>
/// <remarks>
/// This class wraps around the Twilio API client to offer a more manageable interface for sending SMS messages.
/// The wrapper supports both synchronous and asynchronous message sending operations, while also ensuring proper client initialization.
/// </remarks>
public class SmsClientWrapper
{
    private readonly TwilioConfigureOptions _options;

    /// <summary>
    /// Initializes a new instance of the <see cref="SmsClientWrapper"/> class.
    /// This method initializes the Twilio client with the provided configuration options, including the username, password, and Account SID.
    /// </summary>
    /// <param name="options">The configuration options containing Twilio username, password, Account SID, and phone number.</param>
    /// <remarks>
    /// The Twilio client is initialized using the provided credentials to establish a connection with the Twilio service.
    /// This method does not send any messages, but prepares the client for message sending operations.
    /// </remarks>
    public SmsClientWrapper(TwilioConfigureOptions options)
    {
        _options = options;
        TwilioClient.Init(options.Username, options.Password, options.AccountSid);
    }

    /// <summary>
    /// Asynchronously sends an SMS message.
    /// </summary>
    /// <param name="phone">The phone number to send the SMS to.</param>
    /// <param name="message">The message body to send.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    /// <remarks>
    /// This method sends the provided SMS message asynchronously. It uses the Twilio API to send the message to the specified phone number.
    /// </remarks>
    public async Task SendAsync(string phone, string message)
    {
        var messageOptions = new CreateMessageOptions(new PhoneNumber(phone))
        {
            From = new PhoneNumber(_options.PhoneNumber),
            Body = message,
        };

        _ = await MessageResource.CreateAsync(messageOptions);
    }

    /// <summary>
    /// Synchronously sends an SMS message.
    /// </summary>
    /// <param name="phone">The phone number to send the SMS to.</param>
    /// <param name="message">The message body to send.</param>
    /// <remarks>
    /// This method sends the provided SMS message synchronously using the Twilio API.
    /// While this method is blocking, it will wait until the message is sent before returning.
    /// </remarks>
    public void Send(string phone, string message)
    {
        var messageOptions = new CreateMessageOptions(new PhoneNumber(phone))
        {
            From = new PhoneNumber(_options.PhoneNumber),
            Body = message
        };

        _ = MessageResource.Create(messageOptions);
    }
}
