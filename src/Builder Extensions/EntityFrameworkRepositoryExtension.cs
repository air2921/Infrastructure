using Infrastructure.Abstractions;
using Infrastructure.Configuration;
using Infrastructure.Options;
using Infrastructure.Services.EntityFramework;
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
    /// Thrown when the Entity Framework configuration is invalid.
    /// </exception>
    public static IInfrastructureBuilder AddEntityFrameworkRepository<TDbContext>(this IInfrastructureBuilder builder, Action<EntityFrameworkConfigureOptions> configureOptions) where TDbContext : DbContext
    {
        var options = new EntityFrameworkConfigureOptions();
        configureOptions.Invoke(options);

        if (!options.IsEnable)
            return builder;

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
