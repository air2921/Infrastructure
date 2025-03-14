using Infrastructure.Configuration;
using Infrastructure.Enums;

namespace Infrastructure.Options;

public class EntityFrameworkConfigureOptions : Validator
{
    public string Connection { get; set; } = string.Empty;

    public SqlDatabase Database { get; set; }

    public override bool IsValidConfigure()
    {
        if (string.IsNullOrWhiteSpace(Connection))
            return false;

        return true;
    }
}
