namespace Infrastructure.Abstractions;

public interface ISmsSender<TSms> where TSms : notnull
{
    public Task SendSmsAsync(TSms sms);

    public void SendSms(TSms sms);
}
