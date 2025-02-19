using Infrastructure.Exceptions;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Logging;
using MimeKit;
using System.Net.Sockets;

namespace Infrastructure.Services.Smtp;

public class SmtpClientWrapper(
    ILogger<SmtpClientWrapper> logger,
    SmtpConfigureOptions configureOptions)
{
    private readonly SmtpClient _smtpClient = new();

    public async Task EmailSendAsync(MimeMessage message, CancellationToken cancellationToken = default)
    {
        try
        {
            await _smtpClient.ConnectAsync(configureOptions.Provider, configureOptions.Port, SecureSocketOptions.Auto, cancellationToken);
            await _smtpClient.AuthenticateAsync(configureOptions.Address, configureOptions.Password, cancellationToken);
            await _smtpClient.SendAsync(message, cancellationToken);
        }
        catch (Exception ex) when (ex is AuthenticationException || ex is SocketException)
        {
            logger.LogError(ex.ToString(), nameof(EmailSendAsync));
            throw new SmtpClientException();
        }
        catch (OperationCanceledException)
        {
            logger.LogInformation("Email sending operation was cancelled");
        }
        finally
        {
            try
            {
                await _smtpClient.DisconnectAsync(true, cancellationToken);
            }
            catch (OperationCanceledException)
            {
                logger.LogInformation("Disconnect operation was cancelled");
            }
            finally
            {
                _smtpClient.Dispose();
            }
        }
    }
}
