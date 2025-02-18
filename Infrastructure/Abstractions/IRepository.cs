using Infrastructure.Services.EntityFramework.Builder;
using Infrastructure.Services.EntityFramework.Entity;
using System.Linq.Expressions;

namespace Infrastructure.Abstractions;

public interface IRepository<TEntity> where TEntity : EntityBase
{
    public IQueryable<TEntity> GetQuery();

    public ValueTask<int> GetCountAsync(Expression<Func<TEntity, bool>>? filter);

    public Task<IEnumerable<TEntity>> GetRangeAsync(RangeQueryBuilder<TEntity>? builder, CancellationToken cancellationToken = default);

    public Task<TEntity?> GetByFilterAsync(Expression<Func<TEntity, bool>> filter, CancellationToken cancellationToken = default);

    public Task<TEntity?> GetByFilterAsync(SingleQueryBuilder<TEntity> builder, CancellationToken cancellationToken = default);

    public Task<TEntity?> GetByIdAsync(object id, CancellationToken cancellationToken = default);

    public Task<TEntity> AddAsync(TEntity entity, CancellationToken cancellationToken = default);

    public Task AddRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default);

    public Task<TEntity?> DeleteByIdAsync(object id, CancellationToken cancellationToken = default);

    public Task<IEnumerable<TEntity>> DeleteRangeAsync(IEnumerable<object> identifiers, CancellationToken cancellationToken = default);

    public Task<IEnumerable<TEntity>> DeleteRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default);

    public Task<TEntity?> DeleteByFilterAsync(Expression<Func<TEntity, bool>> filter, CancellationToken cancellationToken = default);

    public Task<TEntity?> UpdateAsync(TEntity entity, CancellationToken cancellationToken = default);

    public Task<IEnumerable<TEntity?>> UpdateRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default);
}
