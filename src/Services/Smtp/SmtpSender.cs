using Infrastructure.Abstractions.Exteranal_Services;
using Infrastructure.Data_Transfer_Object;
using Infrastructure.Exceptions;
using Infrastructure.Options;
using Microsoft.Extensions.Logging;
using MimeKit;

namespace Infrastructure.Services.Smtp;

/// <summary>
/// A class responsible for sending emails using an SMTP client. This class implements the <see cref="ISender{MailDetails}"/> interface to send emails asynchronously.
/// </summary>
/// <param name="logger">A logger for tracking errors and operations performed by this class.</param>
/// <param name="configureOptions">Configuration options containing SMTP provider settings like the sender's name and address.</param>
/// <param name="smtpClient">An instance of <see cref="SmtpClientWrapper"/> that handles the actual email sending process.</param>
/// <remarks>
/// This class constructs an email message using the provided details and forwards it to the <see cref="SmtpClientWrapper"/> to send the email.
/// It handles errors during the email construction and sending process and logs any exceptions for further analysis.
/// </remarks>
public class SmtpSender(
    ILogger<SmtpSender> logger,
    SmtpConfigureOptions configureOptions,
    SmtpClientWrapper smtpClient) : ISender<MailDetails>
{
    /// <summary>
    /// Asynchronously sends an email using the provided <see cref="MailDetails"/> object.
    /// </summary>
    /// <param name="mail">An object containing the details of the email to send, including recipient, subject, and body.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <exception cref="SmtpClientException">Thrown if an error occurs while sending the email.</exception>
    public async Task SendAsync(MailDetails mail, CancellationToken cancellationToken = default)
    {
        try
        {
            using var emailMessage = new MimeMessage();
            emailMessage.From.Add(new MailboxAddress(configureOptions.SenderName, configureOptions.Address));
            emailMessage.To.Add(new MailboxAddress(mail.UsernameTo, mail.To));
            emailMessage.Subject = mail.Subject;
            emailMessage.Body = mail.Entity;

            await smtpClient.SendAsync(emailMessage, cancellationToken);
        }
        catch (Exception ex) when (ex is not SmtpClientException)
        {
            logger.LogError(ex, "SMTP error while sending email", [mail.To]);
            throw new SmtpClientException("An error occurred while sending the email");
        }
    }

    /// <summary>
    /// Sends an email synchronously using the provided <see cref="MailDetails"/> object.
    /// </summary>
    /// <param name="mail">An object containing the details of the email to send, including recipient, subject, and body.</param>
    /// <exception cref="SmtpClientException">Thrown if an error occurs while sending the email.</exception>
    public void Send(MailDetails mail)
    {
        try
        {
            using var emailMessage = new MimeMessage();
            emailMessage.From.Add(new MailboxAddress(configureOptions.SenderName, configureOptions.Address));
            emailMessage.To.Add(new MailboxAddress(mail.UsernameTo, mail.To));
            emailMessage.Subject = mail.Subject;
            emailMessage.Body = mail.Entity;

            smtpClient.Send(emailMessage);
        }
        catch (Exception ex) when (ex is not SmtpClientException)
        {
            logger.LogError(ex, "SMTP error while sending email", [mail.To]);
            throw new SmtpClientException("An error occurred while sending the email");
        }
    }
}
