﻿using Infrastructure.Abstractions;
using Infrastructure.Configuration;
using Infrastructure.Data_Transfer_Object;
using Infrastructure.Exceptions.Global;
using Infrastructure.Options;
using Infrastructure.Services.Smtp;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.Builder_Extensions;

/// <summary>
/// Provides extension methods for adding SMTP client services to an <see cref="IInfrastructureBuilder"/>.
/// </summary>
public static class SmtpClientExtension
{
    /// <summary>
    /// Adds SMTP client services to the <see cref="IInfrastructureBuilder"/>.
    /// </summary>
    /// <param name="builder">The infrastructure builder to which the SMTP client services will be added.</param>
    /// <param name="configureOptions">A delegate that configures the SMTP client options.</param>
    /// <returns>The updated <see cref="IInfrastructureBuilder"/> with the added SMTP client services.</returns>
    /// <exception cref="InfrastructureConfigurationException">
    /// Thrown when the SMTP configuration is invalid, such as incorrect Provider, Address, Password, or Port.
    /// </exception>
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