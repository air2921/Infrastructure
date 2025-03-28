using Infrastructure.Abstractions.Utility;
using Infrastructure.Configuration;
using Infrastructure.Data_Transfer_Object.Authorization;
using Infrastructure.Exceptions.Global;
using Infrastructure.Options;
using Infrastructure.Services.Authorization;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.Builder_Extensions;

/// <summary>
/// Provides extension methods for adding authorization services to an <see cref="IInfrastructureBuilder"/>.
/// </summary>
public static class AuthorizationExtension
{
    /// <summary>
    /// Adds authorization services to the <see cref="IInfrastructureBuilder"/>.
    /// </summary>
    /// <param name="builder">The infrastructure builder to which the authorization services will be added.</param>
    /// <param name="configureOptions">A delegate that configures the authorization options.</param>
    /// <returns>The updated <see cref="IInfrastructureBuilder"/> with the added authorization services.</returns>
    /// <exception cref="InfrastructureConfigurationException">
    /// Thrown when the authorization options are invalid or misconfigured.
    /// </exception>
    /// <remarks>
    /// This method registers the following services for Dependency Injection (DI):
    /// <list type="bullet">
    ///     <item><description><see cref="AuthorizationConfigureOptions"/> - Singleton service for storing authorization configuration.</description></item>
    ///     <item><description><see cref="ITokenIssuer{JsonWebTokenDetails}"/> - Scoped service for publishing JSON Web Token details.</description></item>
    ///     <item><description><see cref="ITokenIssuer{RefreshDetails}"/> - Scoped service for publishing refresh token details.</description></item>
    /// </list>
    /// </remarks>
    public static IInfrastructureBuilder AddAuthorization(this IInfrastructureBuilder builder, Action<AuthorizationConfigureOptions> configureOptions)
    {
        var options = new AuthorizationConfigureOptions();
        configureOptions.Invoke(options);

        if (!options.IsEnable)
            return builder;

        options.EnsureSuccessValidation("Invalid auth options, check your configuration");

        builder.Services.AddSingleton(options);
        builder.Services.AddScoped<ITokenIssuer<JsonWebTokenDetails>, JsonWebTokenIssuer>();
        builder.Services.AddScoped<ITokenIssuer<RefreshDetails>, RefreshIssuer>();

        return builder;
    }
}
