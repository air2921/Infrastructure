﻿using Infrastructure.Abstractions.Exteranal_Services;
using Infrastructure.Data_Transfer_Object.Sms;
using Infrastructure.Exceptions;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Services.Sms;

/// <summary>
/// A service for sending SMS messages using a wrapped Twilio client.
/// This class implements the <see cref="ISender{SmsDetails}"/> interface to provide both synchronous and asynchronous message sending functionality.
/// </summary>
/// <remarks>
/// The <see cref="SmsSender"/> class interacts with the <see cref="SmsClientWrapper"/> to send SMS messages. It handles error logging and exception management during the sending process.
/// This class uses a <see cref="Logger{SmsSender}"/> to log errors when an exception occurs while sending messages.
/// </remarks>
/// <param name="smsClient">An instance of <see cref="SmsClientWrapper"/> used to send SMS messages.</param>
/// <remarks>
/// The constructor initializes the <see cref="SmsSender"/> with the provided logger and SMS client. It does not send any messages but prepares the instance for use.
/// </remarks>
public class SmsSender(SmsClientWrapper smsClient) : ISender<SmsDetails>
{
    /// <summary>
    /// Sends an SMS message asynchronously.
    /// </summary>
    /// <param name="sms">The <see cref="SmsDetails"/> object containing the phone number and message body.</param>
    /// <param name="cancellationToken">A token to cancel the operation. This parameter is not used in this method and can be omitted.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    /// <remarks>
    /// This method sends an SMS message asynchronously using the provided <see cref="SmsClientWrapper"/>. 
    /// The cancellation token parameter is not used and can be safely omitted when calling this method.
    /// If any error occurs while sending the SMS, it logs the error and throws a <see cref="SmsClientException"/>.
    /// </remarks>
    /// <exception cref="SmsClientException">Thrown when an error occurs during the SMS sending process.</exception>
    public Task SendAsync(SmsDetails sms, CancellationToken cancellationToken = default)
        => smsClient.SendAsync(sms.To, sms.Message, sms.From);

    /// <summary>
    /// Sends an SMS message synchronously.
    /// </summary>
    /// <param name="sms">The <see cref="SmsDetails"/> object containing the phone number and message body.</param>
    /// <remarks>
    /// This method sends an SMS message synchronously using the provided <see cref="SmsClientWrapper"/>. If any error occurs while sending the SMS,
    /// it logs the error and throws a <see cref="SmsClientException"/>.
    /// </remarks>
    /// <exception cref="SmsClientException">Thrown when an error occurs during the SMS sending process.</exception>
    public void Send(SmsDetails sms)
        => smsClient.Send(sms.To, sms.Message, sms.From);
}
