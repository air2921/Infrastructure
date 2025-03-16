using Infrastructure.Exceptions;

namespace Infrastructure.Abstractions;

/// <summary>
/// Defines a contract for sending emails asynchronously and synchronously via SMTP.
/// </summary>
/// <typeparam name="TMail">The type representing the mail content.</typeparam>
public interface ISmtpSender<TMail> where TMail : notnull
{
    /// <summary>
    /// Sends an email asynchronously.
    /// </summary>
    /// <param name="mail">The mail object containing the details of the email to be sent.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    /// <exception cref="SmtpClientException">Thrown if an error occurs while sending the email.</exception>
    public Task SendMailAsync(TMail mail, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends an email synchronously.
    /// </summary>
    /// <param name="mail">The mail object containing the details of the email to be sent.</param>
    /// <exception cref="SmtpClientException">Thrown if an error occurs while sending the email.</exception>
    public void SendMail(TMail mail);
}
