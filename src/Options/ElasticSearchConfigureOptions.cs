using Infrastructure.Configuration;
using Microsoft.Extensions.Configuration;

namespace Infrastructure.Options;

public class ElasticSearchConfigureOptions : Validator
{
    public string Connection { get; set; } = string.Empty;

    public IConfiguration Configuration { get; set; } = null!;

    public override bool IsValidConfigure()
    {
        if (string.IsNullOrWhiteSpace(Connection))
            return false;

        return true;
    }
}
