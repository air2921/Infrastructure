using Amazon;
using Amazon.Extensions.NETCore.Setup;
using Amazon.Runtime;
using Amazon.S3;
using Infrastructure.Abstractions;
using Infrastructure.Configuration;
using Infrastructure.Data_Transfer_Object;
using Infrastructure.Exceptions.Global;
using Infrastructure.Options;
using Infrastructure.Services.Cryptography;
using Infrastructure.Services.DistributedCache;
using Infrastructure.Services.EntityFramework;
using Infrastructure.Services.EntityFramework.Entity;
using Infrastructure.Services.S3;
using Infrastructure.Services.Smtp;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Sinks.Elasticsearch;
using System.Reflection;

namespace Infrastructure;

public static class AddInfrastructureBuilder
{
    public static IInfrastructureBuilder AddInfrastructure(this IServiceCollection services)
        => new InfrastructureBuilder(services);

    public static IInfrastructureBuilder AddCryptography(this IInfrastructureBuilder builder)
    {
        builder.Services.AddScoped<IHasher, Hasher>();

        return builder;
    }

    public static IInfrastructureBuilder AddElasticSearchLogger(this IInfrastructureBuilder builder, Action<ElasticSearchConfigureOptions> configureOptions)
    {
        var options = new ElasticSearchConfigureOptions();
        configureOptions.Invoke(options);

        if (!options.IsEnable)
            return builder;

        if (!options.IsValidConfigure())
            throw new InfrastructureConfigurationException("Invalid ElasticSearch configuration. Please check connection", nameof(options.Connection));

        const string ASPNETCORE_ENV = "ASPNETCORE_ENVIRONMENT";
        const string ENV_PROPERTY = "Environment";

        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Information()
            .Enrich.FromLogContext()
            .Enrich.WithProperty("Assembly", Assembly.GetExecutingAssembly().GetName().Name)
            .Enrich.WithProperty(ENV_PROPERTY, Environment.GetEnvironmentVariable(ASPNETCORE_ENV)!)
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

    public static IInfrastructureBuilder AddSmtpClient(this IInfrastructureBuilder builder, Action<SmtpConfigureOptions> configureOptions)
    {
        var options = new SmtpConfigureOptions();
        configureOptions.Invoke(options);

        if (!options.IsEnable)
            return builder;

        if (!options.IsValidConfigure())
            throw new InfrastructureConfigurationException("Invalid SMTP configuration. Please check Provider, Address, Password and Port.", nameof(options));

        builder.Services.AddScoped(provider =>
        {
            var logger = provider.GetRequiredService<ILogger<SmtpClientWrapper>>();
            return new SmtpClientWrapper(logger, options);
        });

        builder.Services.AddScoped<ISmtpSender<MailDto>>(provider =>
        {
            var logger = provider.GetRequiredService<ILogger<SmtpSender>>();
            var smtpWrapper = provider.GetRequiredService<SmtpClientWrapper>();
            return new SmtpSender(logger, options, smtpWrapper);
        });

        return builder;
    }

    public static IInfrastructureBuilder AddS3Client(this IInfrastructureBuilder builder, Action<S3ConfigureOptions> configureOptions)
    {
        var options = new S3ConfigureOptions();
        configureOptions.Invoke(options);

        if (!options.IsEnable)
            return builder;

        if (!options.IsValidConfigure())
            throw new InfrastructureConfigurationException("Invalid S3 configuration. Please check AccessKey, SecretKey, and Region.", nameof(options));

        var awsOptions = new AWSOptions
        {
            Credentials = new BasicAWSCredentials(options.AccessKey, options.SecretKey),
            Region = RegionEndpoint.GetBySystemName(options.Region)
        };

        builder.Services.AddAWSService<IAmazonS3>();
        builder.Services.AddDefaultAWSOptions(awsOptions);
        
        builder.Services.AddScoped<IS3Client, S3Client>();

        return builder;
    }

    public static IInfrastructureBuilder AddDistributedCache(this IInfrastructureBuilder builder, Action<CacheConfigureOptions> configureOptions)
    {
        var options = new CacheConfigureOptions();
        configureOptions.Invoke(options);

        if (!options.IsEnable)
            return builder;

        if (!options.IsValidConfigure())
            throw new InfrastructureConfigurationException("Invalid Redis configuration. Please check the connection string.", nameof(options.Connection));

        builder.Services.AddStackExchangeRedisCache(cache =>
        {
            cache.Configuration = options.Connection;
        });

        builder.Services.AddSingleton(options);
        builder.Services.AddScoped<ICacheClient, CacheClient>();

        return builder;
    }

    public static IInfrastructureBuilder AddEntityFrameworkRepository<TDbContext>(this IInfrastructureBuilder builder, Assembly[] assemblies) where TDbContext : DbContext
    {
        builder.Services.AddTransient<ITransactionFactory, TransactionFactory<TDbContext>>();
        builder.Services.AddTransient<ITransactionFactory<TDbContext>, TransactionFactory<TDbContext>>();

        var dbContextType = typeof(TDbContext);
        var baseType = typeof(EntityBase);
        var entityTypes = new List<Type>();

        foreach (var assembly in assemblies)
        {
            var types = assembly.GetTypes();

            foreach (var type in types)
            {
                if (baseType.IsAssignableFrom(type) && type != baseType && !type.IsAbstract)
                    entityTypes.Add(type);
            }
        }

        foreach (var entityType in entityTypes)
        {
            var repositoryType = typeof(Repository<,>).MakeGenericType(entityType, dbContextType);
            var interfaceType = typeof(IRepository<>).MakeGenericType(entityType);
            builder.Services.AddScoped(interfaceType, repositoryType);

            var repositoryWithContextInterfaceType = typeof(IRepository<,>).MakeGenericType(entityType, dbContextType);
            builder.Services.AddScoped(repositoryWithContextInterfaceType, repositoryType);
        }

        return builder;
    }
}
