using Infrastructure.Abstractions;
using Infrastructure.Configuration;
using Infrastructure.Services.EntityFramework.Entity;
using Infrastructure.Services.EntityFramework;
using Microsoft.EntityFrameworkCore;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.Builder_Extensions;

public static class EntityFrameworkRepositoryExtension
{
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
