using Infrastructure.Abstractions;
using Infrastructure.Data_Transfer_Object;
using Infrastructure.Exceptions;
using Infrastructure.Options;
using Microsoft.Extensions.Logging;
using MimeKit;

namespace Infrastructure.Services.Smtp;

/// <summary>
/// A class responsible for sending emails using an SMTP client. This class implements the <see cref="ISmtpSender{MailDetails}"/> interface to send emails asynchronously.
/// </summary>
/// <param name="logger">A logger for tracking errors and operations performed by this class.</param>
/// <param name="configureOptions">Configuration options containing SMTP provider settings like the sender's name and address.</param>
/// <param name="smtpClient">An instance of <see cref="SmtpClientWrapper"/> that handles the actual email sending process.</param>
/// <remarks>
/// This class constructs an email message using the provided details and forwards it to the <see cref="SmtpClientWrapper"/> to send the email.
/// It handles errors during the email construction and sending process and logs any exceptions for further analysis.
/// </remarks>
/// <remarks>
/// Initializes a new instance of the <see cref="SmtpSender"/> class.
/// </remarks>
/// <param name="logger">A logger for tracking errors and operations performed by this class.</param>
/// <param name="configureOptions">Configuration options containing SMTP provider settings like the sender's name and address.</param>
/// <param name="smtpClient">An instance of <see cref="SmtpClientWrapper"/> that handles the actual email sending process.</param>
public class SmtpSender(
    ILogger<SmtpSender> logger,
    SmtpConfigureOptions configureOptions,
    Lazy<SmtpClientWrapper> smtpClient) : ISmtpSender<MailDetails>
{

    /// <summary>
    /// Asynchronously sends an email using the provided <see cref="MailDetails"/> object.
    /// </summary>
    /// <param name="mail">An object containing the details of the email to send, including recipient, subject, and body.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <exception cref="SmtpClientException">Thrown if an error occurs while sending the email.</exception>
    /// <exception cref="ArgumentNullException">Thrown if the <paramref name="mail"/> object is null or contains invalid data.</exception>
    /// <example>
    /// <code>
    /// var smtpSender = new SmtpSender(logger, smtpOptions, smtpClientWrapper);
    /// var mailDetails = new MailDetails { EmailTo = "example@example.com", Subject = "Test", Entity = "Email body content" };
    /// await smtpSender.SendMailAsync(mailDetails);
    /// </code>
    /// </example>
    public async Task SendMailAsync(MailDetails mail, CancellationToken cancellationToken = default)
    {
        try
        {
            var emailMessage = new MimeMessage();
            emailMessage.From.Add(new MailboxAddress(configureOptions.SenderName, configureOptions.Address));
            emailMessage.To.Add(new MailboxAddress(mail.UsernameTo, mail.EmailTo));
            emailMessage.Subject = mail.Subject;
            emailMessage.Body = mail.Entity;

            await smtpClient.Value.EmailSendAsync(emailMessage, cancellationToken);
        }
        catch (SmtpClientException)
        {
            throw;
        }
        catch (Exception ex)
        {
            logger.LogError(ex.Message, mail.EmailTo);
            throw new SmtpClientException("An error occurred while sending the email.");
        }
    }
}
