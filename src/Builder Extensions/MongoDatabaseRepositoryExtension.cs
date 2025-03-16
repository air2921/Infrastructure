using Infrastructure.Abstractions;
using Infrastructure.Configuration;
using Infrastructure.Exceptions.Global;
using Infrastructure.Options;
using Infrastructure.Services.MongoDatabase;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;
using System.Collections.Immutable;

namespace Infrastructure.Builder_Extensions;

/// <summary>
/// Provides extension methods for adding MongoDB repository services to an <see cref="IInfrastructureBuilder"/>.
/// </summary>
public static class MongoDatabaseRepositoryExtension
{
    /// <summary>
    /// Adds MongoDB repository services to the <see cref="IInfrastructureBuilder"/>.
    /// </summary>
    /// <typeparam name="TMongoContext">The type of the MongoDB context used by the repository.</typeparam>
    /// <param name="builder">The infrastructure builder to which the MongoDB repository services will be added.</param>
    /// <param name="configureOptions">A delegate that configures the MongoDB database options.</param>
    /// <returns>The updated <see cref="IInfrastructureBuilder"/> with the added MongoDB repository services.</returns>
    /// <exception cref="InfrastructureConfigurationException">
    /// Thrown when the MongoDB configuration is invalid, such as an incorrect connection string or database.
    /// </exception>
    public static IInfrastructureBuilder AddMongoRepository<TMongoContext>(this IInfrastructureBuilder builder, Action<MongoDatabaseConfigureOptions> configureOptions) where TMongoContext : MongoDatabaseContext
    {
        var options = new MongoDatabaseConfigureOptions();
        configureOptions.Invoke(options);

        if (!options.IsEnable)
            return builder;

        if (!options.IsValidConfigure())
            throw new InfrastructureConfigurationException("Invalid Mongodb configuration. Please check connection or database", nameof(options));

        builder.Services.AddSingleton(options);
        builder.Services.AddScoped<TMongoContext>();

        var mongoContextType = typeof(TMongoContext);
        var documentTypes = mongoContextType.GetProperties()
            .Where(prop => prop.PropertyType.IsGenericType && prop.PropertyType.GetGenericTypeDefinition() == typeof(IMongoCollection<>))
            .Select(prop => prop.PropertyType.GetGenericArguments()[0])
            .ToImmutableArray();

        foreach (var documentType in documentTypes)
        {
            var repositoryType = typeof(MongoDatabaseRepository<,>).MakeGenericType(documentType, mongoContextType);

            var interfaceType = typeof(IMongoRepository<>).MakeGenericType(documentType);
            var repositoryWithContextInterfaceType = typeof(IMongoRepository<,>).MakeGenericType(documentType, mongoContextType);
            builder.Services.AddScoped(repositoryWithContextInterfaceType, repositoryType);
            builder.Services.AddScoped(interfaceType, repositoryType);
        }

        return builder;
    }
}
