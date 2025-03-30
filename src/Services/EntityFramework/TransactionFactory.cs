using Infrastructure.Abstractions.Factory;
using Infrastructure.Services.EntityFramework.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace Infrastructure.Services.EntityFramework;

/// <summary>
/// A factory class to manage database transactions for a given <typeparamref name="TDbContext"/>.
/// <para>Implements <see cref="ITransactionFactory"/> and <see cref="ITransactionFactory{TDbContext}"/> interfaces.</para>
/// </summary>
/// <typeparam name="TDbContext">The type of the database context. It must inherit from <see cref="DbContext"/>.</typeparam>
/// <remarks>
/// Initializes a new instance of the <see cref="TransactionFactory{TDbContext}"/> class.
/// </remarks>
/// <param name="dbContext">The database context to be used for the transaction.</param>
public sealed class TransactionFactory<TDbContext>(TDbContext dbContext) : ITransactionFactory, ITransactionFactory<TDbContext>
    where TDbContext : InfrastructureDbContext
{
    /// <summary>
    /// Begins a new database transaction synchronously.
    /// </summary>
    /// <returns>A new <see cref="IDbContextTransaction"/> representing the transaction.</returns>
    public IDbContextTransaction Begin() => dbContext.Database.BeginTransaction();

    /// <summary>
    /// Begins a new database transaction asynchronously.
    /// </summary>
    /// <param name="cancellationToken">A token to cancel the operation if needed.</param>
    /// <returns>A task that represents the asynchronous operation. The task result is a new <see cref="IDbContextTransaction"/>.</returns>
    public Task<IDbContextTransaction> BeginAsync(CancellationToken cancellationToken = default)
        => dbContext.Database.BeginTransactionAsync(cancellationToken);
}
