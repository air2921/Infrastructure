using Infrastructure.Abstractions;
using Infrastructure.Configuration;
using Infrastructure.Data_Transfer_Object.Authorization;
using Infrastructure.Exceptions.Global;
using Infrastructure.Options;
using Infrastructure.Services.Authorization;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.Builder_Extensions;

public static class AuthorizationExtension
{
    public static IInfrastructureBuilder AddAuthorization(this IInfrastructureBuilder builder, Action<AuthorizationConfigureOptions> configureOptions)
    {
        var options = new AuthorizationConfigureOptions();
        configureOptions.Invoke(options);

        if (!options.IsEnable)
            return builder;

        if (!options.IsValidConfigure())
            throw new InfrastructureConfigurationException("Invalid auth options, check your configuration");

        builder.Services.AddSingleton(options);
        builder.Services.AddScoped<IPublisher<JsonWebTokenDetails>, JsonWebTokenPublisher>();
        builder.Services.AddScoped<IPublisher<RefreshDetails>, RefreshPublisher>();

        return builder;
    }
}
