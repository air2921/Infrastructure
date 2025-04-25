using Infrastructure.Abstractions.Database;
using Infrastructure.Abstractions.External_Services;
using Infrastructure.Exceptions;

namespace Infrastructure.Services.EntityFramework.Context;

/// <summary>
/// Represents a unit of work implementation that coordinates the writing of changes to the underlying database context.
/// </summary>
/// <typeparam name="TDbContext">The type of the database context, which must inherit from EntityFrameworkContext.</typeparam>
/// <param name="context">The database context instance that this unit of work will operate on.</param>
/// <param name="logger">The enhanced logger instance used for logging operations and errors.</param>
public class UnitOfWork<TDbContext>(TDbContext context, ILoggerEnhancer<UnitOfWork<TDbContext>> logger) : IUnitOfWork<TDbContext>, IUnitOfWork where TDbContext : EntityFrameworkContext
{
    /// <summary>
    /// Saves all changes made in this context to the underlying database.
    /// </summary>
    /// <exception cref="EntityException">Thrown when an error occurs while saving changes to the database.</exception>
    public void SaveChanges()
    {
        try
        {
            context.SaveChanges();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unable to save changes", typeof(TDbContext));
            throw new EntityException("Unable to save changes");
        }
    }

    /// <summary>
    /// Asynchronously saves all changes made in this context to the underlying database.
    /// </summary>
    /// <param name="cancellationToken">A token to observe while waiting for the task to complete.</param>
    /// <returns>A task that represents the asynchronous save operation.</returns>
    /// <exception cref="EntityException">Thrown when an error occurs while saving changes to the database.</exception>
    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            await context.SaveChangesAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unable to save changes", typeof(TDbContext));
            throw new EntityException("Unable to save changes");
        }
    }
}
