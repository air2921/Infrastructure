using Infrastructure.Exceptions;
using Infrastructure.Services.EntityFramework.Builder.Base;
using Infrastructure.Services.EntityFramework.Builder.Modules;
using Infrastructure.Services.EntityFramework.Entity;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel;
using System.Linq.Expressions;

namespace Infrastructure.Services.EntityFramework.Builder.Query;

/// <summary>
/// A builder class for constructing queries to retrieve a single entity with various options.
/// </summary>
/// <typeparam name="TEntity">The type of entity to query, must inherit from EntityBase.</typeparam>
public sealed class SingleQueryBuilder<TEntity> : BaseBuilder<SingleQueryBuilder<TEntity>, TEntity> where TEntity : EntityBase
{
    /// <summary>
    /// Private constructor to enforce use of factory method.
    /// </summary>
    private SingleQueryBuilder()
    {
    }

    /// <summary>
    /// Holds the internal state for building include/then-include chains.
    /// </summary>
    internal IncludeChain<TEntity> IncludeChain { get; private set; } = new();

    /// <summary>
    /// Filter expression for the query.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    internal Expression<Func<TEntity, bool>>? Filter { get; private set; }

    /// <summary>
    /// Gets the projection selector expression that transforms the query results.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    internal Expression<Func<TEntity, TEntity>>? Selector { get; private set; }

    /// <summary>
    /// Whether to ignore default query filters.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    internal bool IgnoreDefaultQueryFilters { get; private set; } = false;

    /// <summary>
    /// Indicates whether to use split query behavior to avoid cartesian explosion.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    internal bool AsSplitQuery { get; private set; } = true;

    /// <summary>
    /// A list of sorting expressions and their directions.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    internal IList<(Expression<Func<TEntity, object?>> Expression, bool Descending)> OrderExpressions { get; } = [];

    /// <summary>
    /// Whether to disable change tracking.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    internal bool AsNoTracking { get; private set; } = true;

    /// <summary>
    /// Whether to take the first entity (true) or last entity (false).
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    internal bool TakeFirst { get; private set; } = true;

    /// <summary>
    /// Creates a new instance of SingleQueryBuilder.
    /// </summary>
    public static SingleQueryBuilder<TEntity> Create() => new();

    /// <summary>
    /// Adds a navigation property to be eagerly loaded.
    /// This method can be called multiple times to start separate include paths,
    /// or consecutively to continue a previous include chain (equivalent to ThenInclude).
    /// </summary>
    /// <typeparam name="TProperty">The type of the navigation property.</typeparam>
    /// <param name="navigation">The navigation expression (e.g., x => x.Avatar).</param>
    /// <returns>The current builder instance for chaining.</returns>
    public SingleQueryBuilder<TEntity> WithInclude<TProperty>(
        Expression<Func<TEntity, TProperty>> navigation)
    {
        IncludeChain.AddInclude(navigation);
        return this;
    }

    /// <summary>
    /// Adds a ThenInclude to continue the last Include or ThenInclude path.
    /// Used for including nested reference-type navigation properties.
    /// </summary>
    /// <typeparam name="TPreviousProperty">The type of the previous navigation property.</typeparam>
    /// <typeparam name="TProperty">The type of the next property to include.</typeparam>
    /// <param name="navigation">The navigation expression (e.g., x => x.Address).</param>
    /// <returns>The current builder instance for chaining.</returns>
    public SingleQueryBuilder<TEntity> WithThenInclude<TPreviousProperty, TProperty>(
        Expression<Func<TPreviousProperty, TProperty>> navigation)
    {
        IncludeChain.AddThenInclude(navigation);
        return this;
    }

    /// <summary>
    /// Adds a ThenInclude for nested collection-type navigation properties.
    /// Useful when including collections within included entities.
    /// </summary>
    /// <typeparam name="TPreviousProperty">The type of the previous navigation property.</typeparam>
    /// <typeparam name="TProperty">The type of the collection's element to include.</typeparam>
    /// <param name="navigation">The collection navigation expression (e.g., x => x.Items).</param>
    /// <returns>The current builder instance for chaining.</returns>
    public SingleQueryBuilder<TEntity> WithThenIncludeCollection<TPreviousProperty, TProperty>(
        Expression<Func<TPreviousProperty, IEnumerable<TProperty>>> navigation)
    {
        IncludeChain.AddThenIncludeCollection(navigation);
        return this;
    }

    /// <summary>
    /// Sets a projection selector for the query results.
    /// </summary>
    /// <param name="selector">The projection expression that transforms the query results.</param>
    /// <returns>The current builder instance.</returns>
    public SingleQueryBuilder<TEntity> WithProjection(Expression<Func<TEntity, TEntity>> selector)
    {
        Selector = selector ?? throw new InvalidArgumentException($"Using a {nameof(WithProjection)} without projection expression is not allowed");
        return this;
    }

    /// <summary>
    /// Sets the filter expression for the query.
    /// </summary>
    /// <param name="filter">The filter expression.</param>
    /// <returns>The current builder instance.</returns>
    public SingleQueryBuilder<TEntity> WithFilter(Expression<Func<TEntity, bool>> filter)
    {
        Filter = filter ?? throw new InvalidArgumentException($"Using a {nameof(WithFilter)} without filter expression is not allowed");
        return this;
    }

    /// <summary>
    /// Sets whether to ignore default query filters.
    /// </summary>
    /// <param name="ignore">True to ignore default filters.</param>
    /// <returns>The current builder instance.</returns>
    public SingleQueryBuilder<TEntity> WithIgnoreQueryFilters(bool ignore = true)
    {
        IgnoreDefaultQueryFilters = ignore;
        return this;
    }

    /// <summary>
    /// Sets whether to use split query behavior.
    /// </summary>
    /// <param name="split">True to use split query (recommended when including collections).</param>
    /// <returns>The current builder instance.</returns>
    public SingleQueryBuilder<TEntity> WithSplitQuery(bool split = true)
    {
        AsSplitQuery = split;
        return this;
    }

    /// <summary>
    /// Sets the primary ordering for the query.
    /// </summary>
    /// <param name="orderExpression">The ordering expression.</param>
    /// <param name="descending">True for descending order.</param>
    /// <returns>The current builder instance.</returns>
    public SingleQueryBuilder<TEntity> WithOrdering(
        Expression<Func<TEntity, object?>> orderExpression,
        bool descending = true)
    {
        if (orderExpression == null)
            throw new InvalidArgumentException($"Using a {nameof(WithOrdering)} without order expression is not allowed");

        OrderExpressions.Clear();
        OrderExpressions.Add((orderExpression, descending));
        return this;
    }

    /// <summary>
    /// Adds a secondary ordering for the query.
    /// </summary>
    /// <param name="orderExpression">The ordering expression.</param>
    /// <param name="descending">True for descending order.</param>
    /// <returns>The current builder instance.</returns>
    public SingleQueryBuilder<TEntity> WithThenOrdering(
        Expression<Func<TEntity, object?>> orderExpression,
        bool descending = false)
    {
        if (orderExpression is null)
            throw new InvalidArgumentException($"Using a {nameof(WithThenOrdering)} without order expression is not allowed");

        if (!OrderExpressions.Any())
            throw new InvalidOperationException($"Cannot use {nameof(WithThenOrdering)} without first calling {nameof(WithOrdering)}");

        OrderExpressions.Add((orderExpression, descending));
        return this;
    }

    /// <summary>
    /// Sets whether to disable change tracking.
    /// </summary>
    /// <param name="noTracking">True to disable tracking.</param>
    /// <returns>The current builder instance.</returns>
    public SingleQueryBuilder<TEntity> WithNoTracking(bool noTracking = true)
    {
        AsNoTracking = noTracking;
        return this;
    }

    /// <summary>
    /// Sets whether to take the first entity (true) or last entity (false).
    /// </summary>
    /// <param name="takeFirst">True to take first entity, false for last.</param>
    /// <returns>The current builder instance.</returns>
    public SingleQueryBuilder<TEntity> WithTakeFirst(bool takeFirst = true)
    {
        TakeFirst = takeFirst;
        return this;
    }

    /// <summary>
    /// Applies all configured query parameters to the provided IQueryable.
    /// </summary>
    /// <param name="query">The base query to apply parameters to.</param>
    /// <returns>A new IQueryable with all configured parameters applied.</returns>
    /// <remarks>
    /// The method applies parameters in the following order:
    /// 1. Includes (with full support for multiple and nested navigation paths)
    /// 2. Ignore default query filters (if configured)
    /// 3. Disable change tracking (if configured)
    /// 4. Apply filtering (if specified)
    /// 5. Apply projection (if specified)
    /// 6. Apply ordering (primary + secondary)
    /// 7. Configure split query mode (recommended for collection includes)
    /// Note: This method does not execute the query. You must call a terminal operation
    /// like FirstOrDefault(), SingleOrDefault(), ToList(), etc. on the returned IQueryable.
    /// </remarks>
    public IQueryable<TEntity> Apply(IQueryable<TEntity> query)
    {
        query = IncludeChain.Apply(query);

        if (IgnoreDefaultQueryFilters)
            query = query.IgnoreQueryFilters();

        if (AsNoTracking)
            query = query.AsNoTracking();

        if (AsSplitQuery)
            query = query.AsSplitQuery();

        if (Filter is not null)
            query = query.Where(Filter);

        if (Selector is not null)
            query = query.Select(Selector);

        if (OrderExpressions.Count > 0)
        {
            var orderedQuery = OrderExpressions[0].Descending
                ? query.OrderByDescending(OrderExpressions[0].Expression)
                : query.OrderBy(OrderExpressions[0].Expression);

            for (int i = 1; i < OrderExpressions.Count; i++)
            {
                orderedQuery = OrderExpressions[i].Descending
                    ? orderedQuery.ThenByDescending(OrderExpressions[i].Expression)
                    : orderedQuery.ThenBy(OrderExpressions[i].Expression);
            }

            query = orderedQuery;
        }

        return query;
    }
}