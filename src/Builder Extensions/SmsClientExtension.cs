using Infrastructure.Abstractions.Exteranal_Services;
using Infrastructure.Configuration;
using Infrastructure.Data_Transfer_Object.Sms;
using Infrastructure.Exceptions.Global;
using Infrastructure.Options;
using Infrastructure.Services.Sms;
using Microsoft.Extensions.DependencyInjection;
using Twilio;

namespace Infrastructure.Builder_Extensions;

/// <summary>
/// Provides extension methods for adding SMS client services to an <see cref="IInfrastructureBuilder"/>.
/// </summary>
public static class SmsClientExtension
{
    /// <summary>
    /// Adds SMS client services to the <see cref="IInfrastructureBuilder"/> with the specified configuration.
    /// </summary>
    /// <param name="builder">The infrastructure builder to which the SMS client services will be added.</param>
    /// <param name="configureOptions">A delegate that configures the Twilio SMS client options.</param>
    /// <returns>The updated <see cref="IInfrastructureBuilder"/> with the added SMS client services.</returns>
    /// <exception cref="InfrastructureConfigurationException">
    /// Thrown when the Twilio configuration is invalid, such as incorrect Account SID, Username, Password, or Phone Number.
    /// </exception>
    /// <remarks>
    /// This method registers the following services for Dependency Injection (DI):
    /// <list type="bullet">
    ///     <item><description><see cref="TwilioConfigureOptions"/> - Singleton service for storing Twilio configuration.</description></item>
    ///     <item><description><see cref="SmsClientWrapper"/> - Singleton service for interacting with the Twilio client.</description></item>
    ///     <item><description><see cref="ISender{SmsDetails}"/> - Scoped service for sending SMS messages using the configured Twilio client.</description></item>
    /// </list>
    /// Additionally, this method performs the following:
    /// <list type="bullet">
    ///     <item><description>Validates the Twilio configuration (Account SID, Username, Password, and Phone Number).</description></item>
    ///     <item><description>If the configuration is invalid, throws an <see cref="InfrastructureConfigurationException"/>.</description></item>
    /// </list>
    /// </remarks>
    public static IInfrastructureBuilder AddSmsClient(this IInfrastructureBuilder builder, Action<TwilioConfigureOptions> configureOptions)
    {
        var options = new TwilioConfigureOptions();
        configureOptions.Invoke(options);

        if (!options.IsEnable)
            return builder;

        options.EnsureSuccessValidation("Invalid Twilio configuration. Please check configuration");
        TwilioClient.Init(options.Username, options.Password, options.AccountSid);

        builder.Services.AddSingleton(options);
        builder.Services.AddSingleton<SmsClientWrapper>();
        builder.Services.AddScoped<ISender<SmsDetails>, SmsSender>();

        return builder;
    }
}
