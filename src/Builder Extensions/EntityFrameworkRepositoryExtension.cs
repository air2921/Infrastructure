using Infrastructure.Abstractions;
using Infrastructure.Configuration;
using Infrastructure.Options;
using Infrastructure.Services.EntityFramework;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Immutable;

namespace Infrastructure.Builder_Extensions;

public static class EntityFrameworkRepositoryExtension
{
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
