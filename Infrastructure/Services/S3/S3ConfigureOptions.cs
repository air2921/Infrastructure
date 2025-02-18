using Infrastructure.Configuration;

namespace Infrastructure.Services.S3;

public class S3ConfigureOptions : Validator
{
    public string AccessKey { get; set; } = string.Empty;

    public string SecretKey { get; set; } = string.Empty;

    public string Region { get; set; } = string.Empty;

    public override bool IsValidConfigure()
    {
        if (string.IsNullOrWhiteSpace(AccessKey) || string.IsNullOrWhiteSpace(SecretKey) || string.IsNullOrWhiteSpace(Region))
            return false;

        return true;
    }
}
