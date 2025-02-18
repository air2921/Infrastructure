using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.Configuration;

public interface IInfrastructureBuilder
{
    public IServiceCollection Services { get; set; }
}
