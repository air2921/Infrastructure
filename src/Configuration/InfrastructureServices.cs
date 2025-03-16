using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.Configuration;

/// <summary>
/// Provides a builder for configuring infrastructure services in an application.
/// </summary>
/// <remarks>
/// This class is used to register and configure infrastructure-related services
/// in the dependency injection container.
/// </remarks>
public class InfrastructureBuilder(IServiceCollection services) : IInfrastructureBuilder
{
    /// <summary>
    /// Gets the <see cref="IServiceCollection"/> used to register services.
    /// </summary>
    /// <value>The service collection for registering infrastructure services.</value>
    public IServiceCollection Services { get; private set; } = services;
}
