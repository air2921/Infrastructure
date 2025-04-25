using Infrastructure.Services.EntityFramework.Context;

namespace Infrastructure.Abstractions.Database;

/// <summary>
/// Defines the contract for a unit of work pattern that coordinates the writing of changes to one or more databases.
/// </summary>
public interface IUnitOfWork
{
    /// <summary>
    /// Asynchronously saves all changes made in this unit of work to the underlying database.
    /// </summary>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns>A <see cref="Task"/> that represents the asynchronous save operation.</returns>
    public Task SaveChangesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Saves all changes made in this unit of work to the underlying database.
    /// </summary>
    public void SaveChanges();
}

/// <summary>
/// Defines the contract for a unit of work pattern that operates on a specific <see cref="EntityFrameworkContext"/> database context.
/// </summary>
/// <typeparam name="TDbContext">The type of database context this unit of work operates on, which must inherit from <see cref="EntityFrameworkContext"/>.</typeparam>
public interface IUnitOfWork<TDbContext> : IUnitOfWork where TDbContext : EntityFrameworkContext;
