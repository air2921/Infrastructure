using Infrastructure.Configuration;

namespace Infrastructure.Options;

public class SmtpConfigureOptions : Validator
{
    public string Provider { get; set; } = string.Empty;
    public string SenderName { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public int Port { get; set; }

    public override bool IsValidConfigure()
    {
        if (string.IsNullOrWhiteSpace(Provider) || string.IsNullOrWhiteSpace(Address) ||
            string.IsNullOrWhiteSpace(Password) || string.IsNullOrWhiteSpace(SenderName) || !IsValidPort(Port))
            return false;

        return true;
    }

    private bool IsValidPort(int port)
    {
        int[] ports = [25, 2525, 465, 587];

        if (!ports.Contains(port))
            return false;

        return true;
    }
}
