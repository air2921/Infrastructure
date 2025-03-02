using Infrastructure.Abstractions;
using Infrastructure.Configuration;
using Infrastructure.Services.Utils;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure;

public static class AddInfrastructureBuilder
{
    public static IInfrastructureBuilder AddInfrastructure(this IServiceCollection services)
    {
        services.AddScoped<IGenerator, Generator>();

        return new InfrastructureBuilder(services);
    }
}
