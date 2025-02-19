using Infrastructure.Abstractions;
using Infrastructure.Data_Transfer_Object;
using Infrastructure.Exceptions;
using Microsoft.Extensions.Logging;
using MimeKit;

namespace Infrastructure.Services.Smtp;

public class SmtpSender(
    ILogger<SmtpSender> logger,
    SmtpConfigureOptions configureOptions,
    SmtpClientWrapper smtpClient) : ISmtpSender<MailDto>
{
    public async Task SendMailAsync(MailDto mail, CancellationToken cancellationToken = default)
    {
        try
        {
            var emailMessage = new MimeMessage();
            emailMessage.From.Add(new MailboxAddress(configureOptions.SenderName, configureOptions.Address));
            emailMessage.To.Add(new MailboxAddress(mail.UsernameTo, mail.EmailTo));
            emailMessage.Subject = mail.Subject;
            emailMessage.Body = mail.Entity;

            await smtpClient.EmailSendAsync(emailMessage, cancellationToken);
        }
        catch (SmtpClientException)
        {
            throw;
        }
        catch (Exception ex)
        {
            logger.LogError(ex.Message, mail.EmailTo);
            throw new SmtpClientException();
        }
    }
}
