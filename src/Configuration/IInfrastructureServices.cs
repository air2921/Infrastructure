using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.Configuration;

/// <summary>
/// Defines the contract for an infrastructure builder.
/// </summary>
/// <remarks>
/// This interface provides access to the <see cref="IServiceCollection"/> for
/// registering and configuring infrastructure services.
/// </remarks>
public interface IInfrastructureBuilder
{
    /// <summary>
    /// Gets the <see cref="IServiceCollection"/> used to register services.
    /// </summary>
    /// <value>The service collection for registering infrastructure services.</value>
    public IServiceCollection Services { get; }
}
