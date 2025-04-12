namespace Infrastructure.Abstractions.Database;

public interface IUnitOfWork
{
    public Task SaveChangesAsync(CancellationToken cancellationToken = default);
    public void SaveChanges();
}
