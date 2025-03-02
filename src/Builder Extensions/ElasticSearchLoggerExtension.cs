using Infrastructure.Configuration;
using Infrastructure.Exceptions.Global;
using Infrastructure.Options;
using Serilog.Sinks.Elasticsearch;
using Serilog;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.Builder_Extensions;

public static class ElasticSearchLoggerExtension
{
    public static IInfrastructureBuilder AddElasticSearchLogger(this IInfrastructureBuilder builder, Action<ElasticSearchConfigureOptions> configureOptions)
    {
        var options = new ElasticSearchConfigureOptions();
        configureOptions.Invoke(options);

        if (!options.IsEnable)
            return builder;

        if (!options.IsValidConfigure())
            throw new InfrastructureConfigurationException("Invalid ElasticSearch configuration. Please check connection", nameof(options.Connection));

        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Information()
            .Enrich.FromLogContext()
            .Enrich.WithProperty("Assembly", Assembly.GetExecutingAssembly().GetName().Name)
            .Enrich.WithProperty(Immutable.EnvProperty, Environment.GetEnvironmentVariable(Immutable.AspNetCoreEnv)!)
            .WriteTo.Console()
            .ReadFrom.Configuration(options.Configuration)
            .WriteTo.Elasticsearch(new ElasticsearchSinkOptions(new Uri(options.Connection))
            {
                AutoRegisterTemplate = true,
                IndexFormat = $"logs-{DateTime.UtcNow:yyyy}"
            })
            .CreateLogger();

        builder.Services.AddLogging(logger =>
        {
            logger.AddSerilog(Log.Logger);
        });

        return builder;
    }
}
