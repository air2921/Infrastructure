using Infrastructure.Configuration;
using System.Text;

namespace Infrastructure.Options;

public class AuthorizationOptions : Validator
{
    public Encoding Encoding { get; set; } = Encoding.UTF8;

    public TimeSpan Expiration { get; set; } = TimeSpan.FromMinutes(30);

    public string Key { get; set; } = null!;

    public string Issuer { get; set; } = null!;

    public string Audience { get; set; } = null!;

    public override bool IsValidConfigure()
    {
        if (string.IsNullOrWhiteSpace(Key) || string.IsNullOrWhiteSpace(Issuer) || string.IsNullOrWhiteSpace(Audience))
            return false;

        if (Expiration == TimeSpan.Zero || Expiration.TotalSeconds <= 900)
            return false;

        return true;
    }
}
