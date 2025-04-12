namespace Infrastructure.Abstractions.Database;

/// <summary>
/// Represents a unit of work pattern for managing transactions and coordinating
/// the persistence of changes across multiple repositories.
/// </summary>
/// <remarks>
/// <para>
/// The unit of work maintains a list of objects affected by a business transaction
/// and coordinates the writing out of changes and the resolution of concurrency problems.
/// </para>
/// <para>
/// This interface provides both synchronous and asynchronous methods for saving changes
/// to ensure consistency across all tracked entities.
/// </para>
/// </remarks>
public interface IUnitOfWork
{
    /// <summary>
    /// Asynchronously saves all changes made in this unit of work to the underlying database.
    /// </summary>
    /// <param name="cancellationToken">
    /// A <see cref="CancellationToken"/> to observe while waiting for the task to complete.
    /// </param>
    /// <returns>
    /// A task that represents the asynchronous save operation.
    /// </returns>
    /// <remarks>
    /// <para>
    /// This method will automatically detect changes to tracked entities and persist
    /// all changes (inserts, updates, deletes) to the database in a single transaction.
    /// </para>
    /// <para>
    /// The operation will be canceled if the provided cancellation token is triggered.
    /// </para>
    /// </remarks>
    Task SaveChangesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Synchronously saves all changes made in this unit of work to the underlying database.
    /// </summary>
    /// <remarks>
    /// This method will automatically detect changes to tracked entities and persist
    /// all changes (inserts, updates, deletes) to the database in a single transaction.
    /// Consider using <see cref="SaveChangesAsync"/> for non-blocking operations.
    /// </remarks>
    void SaveChanges();
}
