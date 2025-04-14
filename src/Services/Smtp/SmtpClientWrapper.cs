using Infrastructure.Exceptions;
using Infrastructure.Options;
using MailKit;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Logging;
using MimeKit;
using System.Net.Sockets;

namespace Infrastructure.Services.Smtp;

/// <summary>
/// A wrapper class for the <see cref="SmtpClient"/> that provides a simplified API for sending emails through an SMTP server.
/// This class handles the connection, authentication, and email sending functionality, while also supporting error handling and logging.
/// </summary>
/// <remarks>
/// This class wraps around the <see cref="SmtpClient"/> to provide a safer and more manageable interface for sending emails asynchronously.
/// The email sending process includes exception handling for authentication and network issues, logging them accordingly.
/// The class also implements <see cref="IDisposable"/> to ensure that resources are properly cleaned up after use.
/// </remarks>
public class SmtpClientWrapper : IDisposable
{
    private readonly Lazy<SmtpClient> _transport;
    private readonly ILogger<SmtpClientWrapper> _logger;

    private volatile bool _disposed;

    /// <summary>
    /// Initializes a new instance of the <see cref="SmtpClientWrapper"/> class.
    /// Connects to the SMTP provider and authenticates using the provided configuration options.
    /// </summary>
    /// <param name="logger">A logger for tracking errors and operations performed by this class.</param>
    /// <param name="configureOptions">Configuration options containing SMTP provider, port, address, and password for authentication.</param>
    /// <exception cref="SmtpClientException">Thrown if authentication or connection fails.</exception>
    public SmtpClientWrapper(ILogger<SmtpClientWrapper> logger, SmtpConfigureOptions configureOptions)
    {
        _logger = logger;

        _transport = new Lazy<SmtpClient>(() =>
        {
            var client = new SmtpClient();
            try
            {
                client.Connect(configureOptions.Provider, (int)configureOptions.Port, configureOptions.SecureSocket);
                client.Authenticate(configureOptions.Address, configureOptions.Password);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unable to connect or authorize to smtp server");
                throw new SmtpClientException("Failed to authenticate or connect to the SMTP server");
            }

            return client;
        }, LazyThreadSafetyMode.ExecutionAndPublication);
    }

    /// <summary>
    /// Asynchronously sends an email message.
    /// </summary>
    /// <param name="message">The email message to send.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <exception cref="SmtpClientException">Thrown if there is an error during the email sending process.</exception>
    public async Task SendAsync(MimeMessage message, CancellationToken cancellationToken = default)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        try
        {
            await _transport.Value.SendAsync(message, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex) when (ex is AuthenticationException || ex is SocketException)
        {
            _logger.LogError(ex, "Unable to connect or authorize to smtp server", [message.To.Select(x => x.Name)]);
            throw new SmtpClientException("Error during email sending due to authentication or network issues");
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Email sending operation was cancelled.");
        }
    }

    /// <summary>
    /// Synchronously sends an email message.
    /// </summary>
    /// <param name="message">The email message to send.</param>
    /// <exception cref="SmtpClientException">Thrown if there is an error during the email sending process.</exception>
    public void Send(MimeMessage message)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        try
        {
            _transport.Value.Send(message);
        }
        catch (Exception ex) when (ex is AuthenticationException || ex is SocketException)
        {
            _logger.LogError(ex, "Unable to connect or authorize to smtp server", [message.To.Select(x => x.Name)]);
            throw new SmtpClientException("Error during email sending due to authentication or network issues");
        }
    }

    /// <summary>
    /// Disposes the <see cref="SmtpClientWrapper"/> class, releasing any resources used by the class.
    /// </summary>
    /// <param name="disposing">Indicates whether the method is called from the Dispose method (true) or from the finalizer (false).</param>
    protected virtual void Dispose(bool disposing)
    {
        if (_disposed)
            return;

        if (disposing && _transport.IsValueCreated)
            _transport.Value.Disconnect(true);

        if (_transport.IsValueCreated)
            _transport.Value.Dispose();

        _disposed = true;
    }

    /// <summary>
    /// Disposes the <see cref="SmtpClientWrapper"/> class.
    /// </summary>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
}
