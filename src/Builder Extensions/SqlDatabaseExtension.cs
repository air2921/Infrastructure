using Infrastructure.Configuration;
using Infrastructure.Exceptions.Global;
using Infrastructure.Options;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Builder_Extensions;

internal static class SqlDatabaseExtension
{
    internal static IInfrastructureBuilder AddDatabaseContext<TDbContext>(this IInfrastructureBuilder builder, EntityFrameworkConfigureOptions options) where TDbContext : DbContext
    {
        if (!options.IsEnable)
            return builder;

        if (!options.IsValidConfigure())
            throw new InfrastructureConfigurationException("Invalid Database configuration. Please check connection or SqlType", nameof(options));

        if (options.Database == Enums.SqlDatabase.PostgreSql)
            return builder.AddPostgreSql<TDbContext>(options.Connection);
        if (options.Database == Enums.SqlDatabase.SqlServer)
            return builder.AddSqlServer<TDbContext>(options.Connection);
        if (options.Database == Enums.SqlDatabase.SqlLite)
            return builder.AddSqlLite<TDbContext>(options.Connection);

        throw new InfrastructureConfigurationException("Unknown Sql database", nameof(options.Database));
    }

    private static IInfrastructureBuilder AddPostgreSql<TDbContext>(this IInfrastructureBuilder builder, string connection) where TDbContext : DbContext
    {
        builder.Services.AddDbContextPool<TDbContext>(options =>
        {
            options.UseNpgsql(connection)
            .EnableDetailedErrors(true)
            .LogTo(Console.WriteLine, new[] { DbLoggerCategory.Database.Command.Name }, LogLevel.Information);
        });

        return builder;
    }

    private static IInfrastructureBuilder AddSqlServer<TDbContext>(this IInfrastructureBuilder builder, string connection) where TDbContext : DbContext
    {
        builder.Services.AddDbContextPool<TDbContext>(options =>
        {
            options.UseSqlServer(connection)
            .EnableDetailedErrors(true)
            .LogTo(Console.WriteLine, new[] { DbLoggerCategory.Database.Command.Name }, LogLevel.Information);
        });

        return builder;
    }

    private static IInfrastructureBuilder AddSqlLite<TDbContext>(this IInfrastructureBuilder builder, string connection) where TDbContext : DbContext
    {
        builder.Services.AddDbContextPool<TDbContext>(options =>
        {
            options.UseSqlite(connection)
            .EnableDetailedErrors(true)
            .LogTo(Console.WriteLine, new[] { DbLoggerCategory.Database.Command.Name }, LogLevel.Information);
        });

        return builder;
    }
}
