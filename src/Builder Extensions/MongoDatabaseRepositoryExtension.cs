using Infrastructure.Abstractions.Database;
using Infrastructure.Abstractions.Factory;
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
    /// <remarks>
    /// This method registers the following services for Dependency Injection (DI):
    /// <list type="bullet">
    ///     <item><description><see cref="MongoDatabaseConfigureOptions"/> - Singleton service for storing MongoDB configuration.</description></item>
    ///     <item><description><typeparamref name="TMongoContext"/> - Scoped service for interacting with the MongoDB database context.</description></item>
    ///     <item><description><see cref="IMongoSessionFactory"/> - Scoped service for creating MongoDB sessions.</description></item>
    ///     <item><description><see cref="IMongoSessionFactory{TMongoContext}"/> - Scoped service for creating MongoDB sessions with specific context.</description></item>
    ///     <item><description><see cref="IMongoRepository{TDocument}"/> - Transient service for interacting with documents of type <typeparamref name="TDocument"/>.</description></item>
    ///     <item><description><see cref="IMongoRepository{TMongoContext, TDocument}"/> - Transient service for interacting with documents of type <typeparamref name="TDocument"/> using <typeparamref name="TMongoContext"/>.</description></item>
    /// </list>
    /// Additionally, this method performs the following:
    /// <list type="bullet">
    ///     <item><description>Validates the MongoDB configuration (connection string and database name).</description></item>
    ///     <item><description>Registers repositories for all document types found in <typeparamref name="TMongoContext"/>.</description></item>
    ///     <item><description>Configures session factories for transaction support.</description></item>
    /// </list>
    /// Note: The MongoDB session factories are registered as scoped services to ensure proper transaction handling within a scope.
    /// </remarks>
    public static IInfrastructureBuilder AddMongoRepository<TMongoContext>(this IInfrastructureBuilder builder, Action<MongoDatabaseConfigureOptions> configureOptions) where TMongoContext : MongoDatabaseContext
    {
        var options = new MongoDatabaseConfigureOptions();
        configureOptions.Invoke(options);

        if (!options.IsEnable)
            return builder;

        options.EnsureSuccessValidation("Invalid Mongodb configuration. Please check connection or database");

        builder.Services.AddSingleton(options);
        builder.Services.AddScoped<TMongoContext>();

        builder.Services.AddTransient<IMongoSessionFactory, MongoSessionFactory<TMongoContext>>();
        builder.Services.AddTransient<IMongoSessionFactory<TMongoContext>, MongoSessionFactory<TMongoContext>>();

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
            builder.Services.AddScoped(documentType);
        }

        return builder;
    }
}
