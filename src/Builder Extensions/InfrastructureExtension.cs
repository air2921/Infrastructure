using Infrastructure.Abstractions.Utility;
using Infrastructure.Configuration;
using Infrastructure.Services.Utils;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.Builder_Extensions;

/// <summary>
/// Adds infrastructure services to the service collection.
/// </summary>
public static class AddInfrastructureBuilder
{
    /// <summary>
    /// Registers infrastructure services in the dependency injection container.
    /// </summary>
    /// <param name="services">The service collection to which dependencies are added.</param>
    /// <returns>An <see cref="IInfrastructureBuilder"/> object that provides methods for adding further services.</returns>
    /// <remarks>
    /// This method adds services such as a GUID generator and other infrastructure components to the dependency container. 
    /// The returned <see cref="IInfrastructureBuilder"/> object allows for additional configuration, providing flexibility.
    /// </remarks>
    public static IInfrastructureBuilder AddInfrastructure(this IServiceCollection services)
    {
        services.AddScoped<IRandomizer, Randomizer>();

        return new InfrastructureBuilder(services);
    }
}
