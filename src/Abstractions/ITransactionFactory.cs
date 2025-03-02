using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace Infrastructure.Abstractions;

public interface ITransactionFactory
{
    public IDbContextTransaction Begin();
    public Task<IDbContextTransaction> BeginAsync(CancellationToken cancellationToken = default);
}

public interface ITransactionFactory<TDbContext> where TDbContext : DbContext
{
    public IDbContextTransaction Begin();
    public Task<IDbContextTransaction> BeginAsync(CancellationToken cancellationToken = default);
}
