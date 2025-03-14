using Infrastructure.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace Infrastructure.Services.EntityFramework;

public class TransactionFactory<TDbContext>(TDbContext dbContext) :
    ITransactionFactory,
    ITransactionFactory<TDbContext>
    where TDbContext : DbContext
{
    public IDbContextTransaction Begin() => dbContext.Database.BeginTransaction();
    public async Task<IDbContextTransaction> BeginAsync(CancellationToken cancellationToken = default)
        => await dbContext.Database.BeginTransactionAsync(cancellationToken);
}
