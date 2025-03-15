using Infrastructure.Abstractions;
using Infrastructure.Configuration;
using Infrastructure.Data_Transfer_Object;
using Infrastructure.Exceptions.Global;
using Infrastructure.Options;
using Infrastructure.Services.Smtp;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.Builder_Extensions;

public static class SmtpClientExtension
{
    public static IInfrastructureBuilder AddSmtpClient(this IInfrastructureBuilder builder, Action<SmtpConfigureOptions> configureOptions)
    {
        var options = new SmtpConfigureOptions();
        configureOptions.Invoke(options);

        if (!options.IsEnable)
            return builder;

        if (!options.IsValidConfigure())
            throw new InfrastructureConfigurationException("Invalid SMTP configuration. Please check Provider, Address, Password and Port.", nameof(options));

        builder.Services.AddSingleton(options);
        builder.Services.AddScoped<SmtpClientWrapper>();
        builder.Services.AddScoped<ISmtpSender<MailDetails>, SmtpSender>();

        return builder;
    }
}
