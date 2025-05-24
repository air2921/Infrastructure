using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using System.Data;

namespace Infrastructure.Abstractions.Factory;

/// <summary>
/// Defines a contract for creating and managing database transactions.
/// </summary>
public interface ITransactionFactory
{
    /// <summary>
    /// Begins a new database transaction with the default isolation level.
    /// </summary>
    /// <returns>The <see cref="IDbContextTransaction"/> that represents the new transaction.</returns>
    /// <remarks>
    /// The default isolation level depends on the database provider. For most SQL databases, it's Read Committed.
    /// </remarks>
    public IDbContextTransaction Begin();

    /// <summary>
    /// Begins a new database transaction with the specified isolation level.
    /// </summary>
    /// <param name="isolation">The isolation level to use for the transaction.</param>
    /// <returns>The <see cref="IDbContextTransaction"/> that represents the new transaction.</returns>
    public IDbContextTransaction Begin(IsolationLevel isolation);

    /// <summary>
    /// Asynchronously begins a new database transaction with the default isolation level.
    /// </summary>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the <see cref="IDbContextTransaction"/>.</returns>
    /// <remarks>
    /// The default isolation level depends on the database provider. For most SQL databases, it's Read Committed.
    /// </remarks>
    public Task<IDbContextTransaction> BeginAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Asynchronously begins a new database transaction with the specified isolation level.
    /// </summary>
    /// <param name="isolation">The isolation level to use for the transaction.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the <see cref="IDbContextTransaction"/>.</returns>
    public Task<IDbContextTransaction> BeginAsync(IsolationLevel isolation, CancellationToken cancellationToken = default);
}

/// <summary>
/// Defines a contract for creating and managing database transactions for a specific <typeparamref name="TDbContext"/>.
/// </summary>
/// <typeparam name="TDbContext">The type of the database context.</typeparam>
public interface ITransactionFactory<TDbContext> : ITransactionFactory where TDbContext : DbContext
{
}
