using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace Infrastructure.Abstractions;

/// <summary>
/// Defines a contract for creating and managing database transactions.
/// </summary>
public interface ITransactionFactory
{
    /// <summary>
    /// Begins a new database transaction.
    /// </summary>
    /// <returns>The transaction object.</returns>
    public IDbContextTransaction Begin();

    /// <summary>
    /// Asynchronously begins a new database transaction.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
    /// <returns>A task representing the asynchronous operation. The task result is the transaction object.</returns>
    public Task<IDbContextTransaction> BeginAsync(CancellationToken cancellationToken = default);
}

/// <summary>
/// Defines a contract for creating and managing database transactions for a specific <typeparamref name="TDbContext"/>.
/// </summary>
/// <typeparam name="TDbContext">The type of the database context.</typeparam>
public interface ITransactionFactory<TDbContext> where TDbContext : DbContext
{
    /// <summary>
    /// Begins a new database transaction.
    /// </summary>
    /// <returns>The transaction object.</returns>
    public IDbContextTransaction Begin();

    /// <summary>
    /// Asynchronously begins a new database transaction.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
    /// <returns>A task representing the asynchronous operation. The task result is the transaction object.</returns>
    public Task<IDbContextTransaction> BeginAsync(CancellationToken cancellationToken = default);
}
