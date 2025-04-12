using Infrastructure.Abstractions.Database;
using Infrastructure.Abstractions.Factory;
using Infrastructure.Configuration;
using Infrastructure.Exceptions.Global;
using Infrastructure.Options;
using Infrastructure.Services.EntityFramework;
using Infrastructure.Services.EntityFramework.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Immutable;

namespace Infrastructure.Builder_Extensions;

/// <summary>
/// Provides extension methods for adding Entity Framework repository services to an <see cref="IInfrastructureBuilder"/>.
/// </summary>
public static class EntityFrameworkRepositoryExtension
{
    /// <summary>
    /// Adds Entity Framework repository services to the <see cref="IInfrastructureBuilder"/>.
    /// </summary>
    /// <typeparam name="TDbContext">The type of the DbContext used by the repository.</typeparam>
    /// <param name="builder">The infrastructure builder to which the Entity Framework repository services will be added.</param>
    /// <param name="configureOptions">A delegate that configures the Entity Framework options.</param>
    /// <returns>The updated <see cref="IInfrastructureBuilder"/> with the added Entity Framework repository services.</returns>
    /// <exception cref="InfrastructureConfigurationException">
    /// Thrown when the database configuration is invalid, such as an incorrect connection string or database type.
    /// </exception>
    /// <remarks>
    /// This method registers the following services for Dependency Injection (DI):
    /// <list type="bullet">
    ///     <item><description><see cref="EntityFrameworkConfigureOptions"/> - Singleton service for storing Entity Framework configuration.</description></item>
    ///     <item><description><see cref="ITransactionFactory"/> - Transient service for creating transactions.</description></item>
    ///     <item><description><see cref="ITransactionFactory{TDbContext}"/> - Transient service for creating transactions specific to <typeparamref name="TDbContext"/>.</description></item>
    ///     <item><description>Scoped services for <see cref="IRepository{T}"/> and <see cref="IRepository{T, TDbContext}"/> for each entity type in <typeparamref name="TDbContext"/>.</description></item>
    /// </list>
    /// Additionally, this method configures the database context based on the specified SQL database type:
    /// <list type="bullet">
    ///     <item><description>PostgreSQL: Configures <typeparamref name="TDbContext"/> to use Npgsql with detailed errors and logging.</description></item>
    ///     <item><description>SQL Server: Configures <typeparamref name="TDbContext"/> to use SQL Server with detailed errors and logging.</description></item>
    ///     <item><description>SQLite: Configures <typeparamref name="TDbContext"/> to use SQLite with detailed errors and logging.</description></item>
    /// </list>
    /// <para>
    /// Note: This method automatically registers repository services for all entity types (DbSet&lt;T&gt;) found in the specified <typeparamref name="TDbContext"/>.
    /// </para>
    /// </remarks>
    public static IInfrastructureBuilder AddEntityFrameworkRepository<TDbContext>(this IInfrastructureBuilder builder, Action<EntityFrameworkConfigureOptions> configureOptions) where TDbContext : EntityFrameworkContext
    {
        var options = new EntityFrameworkConfigureOptions();
        configureOptions.Invoke(options);

        if (!options.IsEnable)
            return builder;

        options.EnsureSuccessValidation("Invalid database configuration, check connection string");

        builder.AddDatabaseContext<TDbContext>(options);

        builder.Services.AddTransient<ITransactionFactory, TransactionFactory<TDbContext>>();
        builder.Services.AddTransient<ITransactionFactory<TDbContext>, TransactionFactory<TDbContext>>();

        var dbContextType = typeof(TDbContext);
        var entityTypes = dbContextType.GetProperties()
            .Where(prop => prop.PropertyType.IsGenericType && prop.PropertyType.GetGenericTypeDefinition() == typeof(DbSet<>))
            .Select(prop => prop.PropertyType.GetGenericArguments()[0])
            .ToImmutableArray();

        foreach (var entityType in entityTypes)
        {
            var repositoryType = typeof(Repository<,>).MakeGenericType(entityType, dbContextType);

            var interfaceType = typeof(IRepository<>).MakeGenericType(entityType);
            var repositoryWithContextInterfaceType = typeof(IRepository<,>).MakeGenericType(entityType, dbContextType);
            builder.Services.AddScoped(repositoryWithContextInterfaceType, repositoryType);
            builder.Services.AddScoped(interfaceType, repositoryType);
        }

        return builder;
    }
}
