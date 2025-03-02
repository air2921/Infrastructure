using Infrastructure.Services.EntityFramework.Entity;
using System.Linq.Expressions;

namespace Infrastructure.Services.EntityFramework.Builder;

public class RangeQueryBuilder<TEntity> : EntityBase
{
    /// <summary>
    /// An expression for filtering entities.
    /// </summary>
    public Expression<Func<TEntity, bool>>? Filter { get; init; }

    /// <summary>
    /// An expression for sorting entities.
    /// </summary>
    public Expression<Func<TEntity, object?>>? OrderExpression { get; init; }

    /// <summary>
    /// A query for including related entities (e.g., using the <see cref="Queryable.Include"/> method).
    /// </summary>
    public IQueryable<TEntity>? IncludeQuery { get; init; }

    /// <summary>
    /// Indicates whether the <see cref="Queryable.AsNoTracking"/> method should be used to disable change tracking.
    /// </summary>
    public bool AsNoTracking { get; set; } = false;

    /// <summary>
    /// Indicates whether entities should be sorted in descending order (default is descending).
    /// </summary>
    public bool OrderByDesc { get; set; } = true;

    /// <summary>
    /// The number of entities to be skipped in the query result (for pagination).
    /// </summary>
    public int Skip { get; set; } = 0;

    /// <summary>
    /// The number of entities to be retrieved in the query result.
    /// </summary>
    public int Take { get; set; } = 1000;
}
