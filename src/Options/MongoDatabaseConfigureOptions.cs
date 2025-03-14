using Infrastructure.Configuration;

namespace Infrastructure.Options;

public class MongoDatabaseConfigureOptions : Validator
{
    public string Connection { get; set; } = string.Empty;

    public string Database { get; set; } = string.Empty;

    public override bool IsValidConfigure()
    {
        if (string.IsNullOrWhiteSpace(Connection) || string.IsNullOrWhiteSpace(Database))
            return false;

        return true;
    }
}
