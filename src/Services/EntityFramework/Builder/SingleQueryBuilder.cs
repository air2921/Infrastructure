using Infrastructure.Services.EntityFramework.Entity;
using System.Linq.Expressions;

namespace Infrastructure.Services.EntityFramework.Builder;

/// <summary>
/// A builder class for constructing queries to retrieve a single entity with various options.
/// </summary>
/// <typeparam name="TEntity">The type of entity to query, must inherit from EntityBase.</typeparam>
public class SingleQueryBuilder<TEntity> where TEntity : EntityBase
{
    private SingleQueryBuilder()
    {
        
    }

    /// <summary>
    /// Filter expression for the query.
    /// </summary>
    public Expression<Func<TEntity, bool>>? Filter { get; private set; }

    /// <summary>
    /// Whether to ignore default query filters.
    /// </summary>
    public bool IgnoreDefaultQuerySettings { get; private set; }

    /// <summary>
    /// Ordering expression for the query.
    /// </summary>
    public Expression<Func<TEntity, object?>>? OrderExpression { get; private set; }

    /// <summary>
    /// Include query for related entities.
    /// </summary>
    public IQueryable<TEntity>? IncludeQuery { get; private set; }

    /// <summary>
    /// Whether to disable change tracking.
    /// </summary>
    public bool AsNoTracking { get; private set; }

    /// <summary>
    /// Whether to sort in descending order.
    /// </summary>
    public bool OrderByDesc { get; private set; } = true;

    /// <summary>
    /// Whether to take the first entity (true) or last entity (false).
    /// </summary>
    public bool TakeFirst { get; private set; } = true;

    /// <summary>
    /// Creates a new instance of SingleQueryBuilder.
    /// </summary>
    public static SingleQueryBuilder<TEntity> Create() => new();

    /// <summary>
    /// Sets the filter expression for the query.
    /// </summary>
    /// <param name="filter">The filter expression.</param>
    public SingleQueryBuilder<TEntity> WithFilter(Expression<Func<TEntity, bool>> filter)
    {
        Filter = filter ?? throw new ArgumentNullException(nameof(filter));
        return this;
    }

    /// <summary>
    /// Sets whether to ignore default query filters.
    /// </summary>
    /// <param name="ignore">True to ignore default filters.</param>
    public SingleQueryBuilder<TEntity> WithIgnoreQueryFilters(bool ignore = true)
    {
        IgnoreDefaultQuerySettings = ignore;
        return this;
    }

    /// <summary>
    /// Sets the ordering for the query.
    /// </summary>
    /// <param name="orderExpression">The ordering expression.</param>
    /// <param name="descending">True for descending order.</param>
    public SingleQueryBuilder<TEntity> WithOrdering(
        Expression<Func<TEntity, object?>> orderExpression,
        bool descending = true)
    {
        OrderExpression = orderExpression ?? throw new ArgumentNullException(nameof(orderExpression));
        OrderByDesc = descending;
        return this;
    }

    /// <summary>
    /// Sets the include query for related entities.
    /// </summary>
    /// <param name="includeQuery">The include query.</param>
    public SingleQueryBuilder<TEntity> WithIncludes(IQueryable<TEntity> includeQuery)
    {
        IncludeQuery = includeQuery ?? throw new ArgumentNullException(nameof(includeQuery));
        return this;
    }

    /// <summary>
    /// Sets whether to disable change tracking.
    /// </summary>
    /// <param name="noTracking">True to disable tracking.</param>
    public SingleQueryBuilder<TEntity> WithNoTracking(bool noTracking = true)
    {
        AsNoTracking = noTracking;
        return this;
    }

    /// <summary>
    /// Sets whether to take the first entity (true) or last entity (false).
    /// </summary>
    /// <param name="takeFirst">True to take first entity, false for last.</param>
    public SingleQueryBuilder<TEntity> WithTakeFirst(bool takeFirst = true)
    {
        TakeFirst = takeFirst;
        return this;
    }

    /// <summary>
    /// Validates the builder configuration.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when configuration is invalid.</exception>
    public void Validate()
    {
        if (Filter is null && OrderExpression is null)
        {
            throw new InvalidOperationException(
                "Either Filter or OrderExpression must be specified for single entity query");
        }
    }
}
