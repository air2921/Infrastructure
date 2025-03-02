using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.Configuration;

public class InfrastructureBuilder(IServiceCollection services) : IInfrastructureBuilder
{
    public IServiceCollection Services { get; } = services;
}
