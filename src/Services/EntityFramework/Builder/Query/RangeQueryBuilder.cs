using Infrastructure.Exceptions;
using Infrastructure.Services.EntityFramework.Entity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using System.ComponentModel;
using System.Linq.Expressions;

namespace Infrastructure.Services.EntityFramework.Builder.Query;

/// <summary>
/// A class that helps build queries for filtering, sorting, and including related entities in a range query.
/// <para>This class is designed to assist with pagination and custom queries for entities of type <typeparamref name="TEntity"/>.</para>
/// </summary>
/// <typeparam name="TEntity">The type of the entity to query.</typeparam>
public sealed class RangeQueryBuilder<TEntity> : BaseBuilder<RangeQueryBuilder<TEntity>, TEntity> where TEntity : EntityBase
{
    /// <summary>
    /// Private constructor to enforce use of factory method.
    /// </summary>
    private RangeQueryBuilder()
    {
    }

    /// <summary>
    /// An expression for filtering entities based on a condition.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    internal Expression<Func<TEntity, bool>>? Filter { get; private set; }

    /// <summary>
    /// Indicates whether the query should ignore default query filters.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    internal bool IgnoreDefaultQueryFilters { get; private set; } = false;

    /// <summary>
    /// Indicates whether to use split query behavior to avoid cartesian explosion.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    internal bool AsSplitQuery { get; private set; } = true;

    /// <summary>
    /// An expression for sorting entities.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    internal Expression<Func<TEntity, object?>>? OrderExpression { get; private set; }

    /// <summary>
    /// A query for including related entities.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    internal IIncludableQueryable<TEntity, object?>? IncludeQuery { get; private set; }

    /// <summary>
    /// Indicates whether change tracking should be disabled.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    internal bool AsNoTracking { get; private set; } = true;

    /// <summary>
    /// Indicates sorting direction.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    internal bool OrderByDesc { get; private set; } = true;

    /// <summary>
    /// The number of entities to skip.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    internal int Skip { get; private set; } = 0;

    /// <summary>
    /// The number of entities to take.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    internal int Take { get; private set; } = 100;

    /// <summary>
    /// Creates a new instance of RangeQueryBuilder with default settings.
    /// </summary>
    public static RangeQueryBuilder<TEntity> Create() => new();

    /// <summary>
    /// Sets the filter expression for the query.
    /// </summary>
    /// <param name="filter">The filter expression.</param>
    /// <returns>The current builder instance.</returns>
    public RangeQueryBuilder<TEntity> WithFilter(Expression<Func<TEntity, bool>> filter)
    {
        Filter = filter ?? throw new InvalidArgumentException($"Using a {nameof(WithFilter)} without filter expression is not allowed");
        return this;
    }

    /// <summary>
    /// Sets whether to ignore default query filters.
    /// </summary>
    /// <param name="ignore">True to ignore default filters.</param>
    /// <returns>The current builder instance.</returns>
    public RangeQueryBuilder<TEntity> WithIgnoreQueryFilters(bool ignore = true)
    {
        IgnoreDefaultQueryFilters = ignore;
        return this;
    }

    /// <summary>
    /// Sets whether to use split query behavior.
    /// </summary>
    /// <param name="split">True to use split query (recommended when including collections).</param>
    /// <returns>The current builder instance.</returns>
    public RangeQueryBuilder<TEntity> WithSplitQuery(bool split = true)
    {
        AsSplitQuery = split;
        return this;
    }

    /// <summary>
    /// Sets the ordering expression and direction.
    /// </summary>
    /// <param name="orderExpression">The ordering expression.</param>
    /// <param name="descending">True for descending order.</param>
    /// <returns>The current builder instance.</returns>
    public RangeQueryBuilder<TEntity> WithOrdering(
        Expression<Func<TEntity, object?>> orderExpression,
        bool descending = true)
    {
        OrderExpression = orderExpression ?? throw new InvalidArgumentException($"Using a {nameof(WithOrdering)} without order expression is not allowed");
        OrderByDesc = descending;
        return this;
    }

    /// <summary>
    /// Sets the include query for related entities.
    /// </summary>
    /// <param name="includeQuery">The include query.</param>
    /// <returns>The current builder instance.</returns>
    public RangeQueryBuilder<TEntity> WithIncludes(IIncludableQueryable<TEntity, object?> includeQuery)
    {
        IncludeQuery = includeQuery ?? throw new InvalidArgumentException($"Using a {nameof(WithIncludes)} without includable expression is not allowed");
        return this;
    }

    /// <summary>
    /// Sets whether to disable change tracking.
    /// </summary>
    /// <param name="noTracking">True to disable tracking.</param>
    /// <returns>The current builder instance.</returns>
    public RangeQueryBuilder<TEntity> WithNoTracking(bool noTracking = true)
    {
        AsNoTracking = noTracking;
        return this;
    }

    /// <summary>
    /// Sets the pagination parameters.
    /// </summary>
    /// <param name="skip">Number of entities to skip.</param>
    /// <param name="take">Number of entities to take.</param>
    /// <returns>The current builder instance.</returns>
    public RangeQueryBuilder<TEntity> WithPagination(int skip, int take)
    {
        if (skip < 0)
            throw new InvalidArgumentException($"Using a {nameof(WithPagination)} with {nameof(skip)} param less than zero is not allowed");

        if (take <= 0)
            throw new InvalidArgumentException($"Using a {nameof(WithPagination)} with {nameof(take)} param less or zero is not allowed");

        if (take > 1000 && !IsIgnoredBuilderConstraints)
            throw new InvalidArgumentException($"Using a {nameof(WithPagination)} with {nameof(take)} param more than 1000 is not allowed");

        Skip = skip;
        Take = take;
        return this;
    }

    /// <summary>
    /// Applies all configured query parameters to the provided IQueryable.
    /// </summary>
    /// <param name="query">The base query to apply parameters to.</param>
    /// <returns>A new IQueryable with all configured parameters applied.</returns>
    /// <remarks>
    /// The method applies parameters in the following order:
    /// 1. Include related entities (if specified)
    /// 2. Ignore default query filters (if configured)
    /// 3. Disable change tracking (if configured)
    /// 4. Apply filtering (if specified)
    /// 5. Apply sorting (if specified)
    /// 6. Apply pagination (skip/take)
    /// 7. Configure as split query (to avoid cartesian explosion when including collections)
    /// </remarks>
    public IQueryable<TEntity> Apply(IQueryable<TEntity> query)
    {
        if (IncludeQuery is not null)
            query = IncludeQuery;

        if (IgnoreDefaultQueryFilters)
            query = query.IgnoreQueryFilters();

        if (AsNoTracking)
            query = query.AsNoTracking();

        if (AsSplitQuery)
            query = query.AsSplitQuery();

        if (Filter is not null)
            query = query.Where(Filter);

        if (OrderExpression is not null)
            query = OrderByDesc ? query.OrderByDescending(OrderExpression) : query.OrderBy(OrderExpression);

        query = query.Skip(Skip).Take(Take);

        return query;
    }
}
