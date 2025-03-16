using Infrastructure.Configuration;
using Infrastructure.Exceptions.Global;
using Infrastructure.Options;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Builder_Extensions;

/// <summary>
/// Provides extension methods for adding database context services for different SQL databases to an <see cref="IInfrastructureBuilder"/>.
/// </summary>
internal static class SqlDatabaseExtension
{
    /// <summary>
    /// Adds a database context service to the <see cref="IInfrastructureBuilder"/> based on the specified SQL database type.
    /// </summary>
    /// <typeparam name="TDbContext">The type of the database context to be added.</typeparam>
    /// <param name="builder">The infrastructure builder to which the database context service will be added.</param>
    /// <param name="options">The configuration options for the database connection.</param>
    /// <returns>The updated <see cref="IInfrastructureBuilder"/> with the added database context service.</returns>
    /// <exception cref="InfrastructureConfigurationException">
    /// Thrown when the database configuration is invalid, such as an incorrect connection string or database type.
    /// </exception>
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

    /// <summary>
    /// Adds PostgreSQL database context to the <see cref="IInfrastructureBuilder"/>.
    /// </summary>
    /// <typeparam name="TDbContext">The type of the database context to be added.</typeparam>
    /// <param name="builder">The infrastructure builder to which the PostgreSQL database context service will be added.</param>
    /// <param name="connection">The connection string for the PostgreSQL database.</param>
    /// <returns>The updated <see cref="IInfrastructureBuilder"/> with the added PostgreSQL database context service.</returns>
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

    /// <summary>
    /// Adds SQL Server database context to the <see cref="IInfrastructureBuilder"/>.
    /// </summary>
    /// <typeparam name="TDbContext">The type of the database context to be added.</typeparam>
    /// <param name="builder">The infrastructure builder to which the SQL Server database context service will be added.</param>
    /// <param name="connection">The connection string for the SQL Server database.</param>
    /// <returns>The updated <see cref="IInfrastructureBuilder"/> with the added SQL Server database context service.</returns>
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

    /// <summary>
    /// Adds SQLite database context to the <see cref="IInfrastructureBuilder"/>.
    /// </summary>
    /// <typeparam name="TDbContext">The type of the database context to be added.</typeparam>
    /// <param name="builder">The infrastructure builder to which the SQLite database context service will be added.</param>
    /// <param name="connection">The connection string for the SQLite database.</param>
    /// <returns>The updated <see cref="IInfrastructureBuilder"/> with the added SQLite database context service.</returns>
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
