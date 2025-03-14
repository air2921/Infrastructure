using Infrastructure.Services.EntityFramework.Entity;
using System.Linq.Expressions;

namespace Infrastructure.Services.EntityFramework.Builder;

/// <summary>
/// A class that helps build queries for filtering, sorting, and including related entities in a range query.
/// <para>This class is designed to assist with pagination and custom queries for entities of type <typeparamref name="TEntity"/>.</para>
/// </summary>
/// <typeparam name="TEntity">The type of the entity to query.</typeparam>
public class RangeQueryBuilder<TEntity> : EntityBase
{
    /// <summary>
    /// An expression for filtering entities based on a condition.
    /// <para>This expression is used to filter the entities that meet the specified condition.</para>
    /// </summary>
    public Expression<Func<TEntity, bool>>? Filter { get; init; }

    /// <summary>
    /// An expression for sorting entities.
    /// <para>This expression specifies the property by which the entities should be ordered.</para>
    /// </summary>
    public Expression<Func<TEntity, object?>>? OrderExpression { get; init; }

    /// <summary>
    /// A query for including related entities (e.g., using the <see cref="Queryable.Include"/> method).
    /// <para>This query can be used to eagerly load related entities in the result set.</para>
    /// </summary>
    public IQueryable<TEntity>? IncludeQuery { get; init; }

    /// <summary>
    /// Indicates whether the <see cref="Queryable.AsNoTracking"/> method should be used to disable change tracking.
    /// <para>If set to <c>true</c>, the query will not track changes to the entities, which may improve performance.</para>
    /// </summary>
    public bool AsNoTracking { get; set; } = false;

    /// <summary>
    /// Indicates whether entities should be sorted in descending order (default is descending).
    /// <para>If set to <c>true</c>, the results will be ordered in descending order; otherwise, they will be in ascending order.</para>
    /// </summary>
    public bool OrderByDesc { get; set; } = true;

    /// <summary>
    /// The number of entities to be skipped in the query result (for pagination).
    /// <para>This property is used to skip the first <see cref="Skip"/> number of entities when applying pagination.</para>
    /// </summary>
    public int Skip { get; set; } = 0;

    /// <summary>
    /// The number of entities to be retrieved in the query result.
    /// <para>This property limits the number of entities returned by the query, used for pagination.</para>
    /// </summary>
    public int Take { get; set; } = 1000;
}
