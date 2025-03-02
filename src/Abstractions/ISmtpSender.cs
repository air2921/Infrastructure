namespace Infrastructure.Abstractions;

public interface ISmtpSender<TMail> where TMail : notnull
{
    public Task SendMailAsync(TMail mail, CancellationToken cancellationToken = default);
}
