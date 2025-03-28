﻿using Infrastructure.Services.EntityFramework.Entity;
using System.Linq.Expressions;

namespace Infrastructure.Services.EntityFramework.Builder;

/// <summary>
/// A class that helps build queries for filtering, sorting, and including related entities in a range query.
/// <para>This class is designed to assist with pagination and custom queries for entities of type <typeparamref name="TEntity"/>.</para>
/// </summary>
/// <typeparam name="TEntity">The type of the entity to query.</typeparam>
public class RangeQueryBuilder<TEntity> where TEntity : EntityBase
{
    private RangeQueryBuilder()
    {
        
    }

    /// <summary>
    /// An expression for filtering entities based on a condition.
    /// </summary>
    public Expression<Func<TEntity, bool>>? Filter { get; private set; }

    /// <summary>
    /// Indicates whether the query should ignore default query filters.
    /// </summary>
    public bool IgnoreDefaultQuerySettings { get; private set; }

    /// <summary>
    /// An expression for sorting entities.
    /// </summary>
    public Expression<Func<TEntity, object?>>? OrderExpression { get; private set; }

    /// <summary>
    /// A query for including related entities.
    /// </summary>
    public IQueryable<TEntity>? IncludeQuery { get; private set; }

    /// <summary>
    /// Indicates whether change tracking should be disabled.
    /// </summary>
    public bool AsNoTracking { get; private set; }

    /// <summary>
    /// Indicates sorting direction.
    /// </summary>
    public bool OrderByDesc { get; private set; } = true;

    /// <summary>
    /// The number of entities to skip.
    /// </summary>
    public int Skip { get; private set; }

    /// <summary>
    /// The number of entities to take.
    /// </summary>
    public int Take { get; private set; } = 1000;

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
        Filter = filter ?? throw new ArgumentNullException(nameof(filter));
        return this;
    }

    /// <summary>
    /// Sets whether to ignore default query filters.
    /// </summary>
    /// <param name="ignore">True to ignore default filters.</param>
    /// <returns>The current builder instance.</returns>
    public RangeQueryBuilder<TEntity> WithIgnoreQueryFilters(bool ignore = true)
    {
        IgnoreDefaultQuerySettings = ignore;
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
        OrderExpression = orderExpression ?? throw new ArgumentNullException(nameof(orderExpression));
        OrderByDesc = descending;
        return this;
    }

    /// <summary>
    /// Sets the include query for related entities.
    /// </summary>
    /// <param name="includeQuery">The include query.</param>
    /// <returns>The current builder instance.</returns>
    public RangeQueryBuilder<TEntity> WithIncludes(IQueryable<TEntity> includeQuery)
    {
        IncludeQuery = includeQuery ?? throw new ArgumentNullException(nameof(includeQuery));
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
        ArgumentOutOfRangeException.ThrowIfNegative(skip);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(take);

        Skip = skip;
        Take = take;
        return this;
    }
}
