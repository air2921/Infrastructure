﻿using Infrastructure.Exceptions;
using Infrastructure.Services.EntityFramework.Entity;
using Microsoft.EntityFrameworkCore.Query;
using System.ComponentModel;
using System.Linq.Expressions;

namespace Infrastructure.Services.EntityFramework.Builder;

/// <summary>
/// A builder class for constructing queries to retrieve a single entity with various options.
/// </summary>
/// <typeparam name="TEntity">The type of entity to query, must inherit from EntityBase.</typeparam>
public sealed class SingleQueryBuilder<TEntity> where TEntity : EntityBase
{
    /// <summary>
    /// Private constructor to enforce use of factory method.
    /// </summary>
    private SingleQueryBuilder()
    {
        
    }

    /// <summary>
    /// Filter expression for the query.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    internal Expression<Func<TEntity, bool>>? Filter { get; private set; }

    /// <summary>
    /// Whether to ignore default query filters.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    internal bool IgnoreDefaultQuerySettings { get; private set; } = false;

    /// <summary>
    /// Ordering expression for the query.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    internal Expression<Func<TEntity, object?>>? OrderExpression { get; private set; }

    /// <summary>
    /// Include query for related entities.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    internal IIncludableQueryable<TEntity, object?>? IncludeQuery { get; private set; }

    /// <summary>
    /// Whether to disable change tracking.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    internal bool AsNoTracking { get; private set; } = true;

    /// <summary>
    /// Whether to sort in descending order.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    internal bool OrderByDesc { get; private set; } = true;

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
    /// Sets the filter expression for the query.
    /// </summary>
    /// <param name="filter">The filter expression.</param>
    public SingleQueryBuilder<TEntity> WithFilter(Expression<Func<TEntity, bool>> filter)
    {
        Filter = filter ?? throw new InvalidArgumentException($"Using a {nameof(WithFilter)} without filter expression is not allowed");
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
        OrderExpression = orderExpression ?? throw new InvalidArgumentException($"Using a {nameof(WithOrdering)} without order expression is not allowed");
        OrderByDesc = descending;
        return this;
    }

    /// <summary>
    /// Sets the include query for related entities.
    /// </summary>
    /// <param name="includeQuery">The include query.</param>
    public SingleQueryBuilder<TEntity> WithIncludes(IIncludableQueryable<TEntity, object?> includeQuery)
    {
        IncludeQuery = includeQuery ?? throw new InvalidArgumentException($"Using a {nameof(WithIncludes)} without  includable expression is not allowed");
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
}
