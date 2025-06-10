using Infrastructure.Services.EntityFramework.Entity;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using Infrastructure.Exceptions;

namespace Infrastructure.Services.EntityFramework.Builder.Modules;

/// <summary>
/// Represents a chainable builder for configuring Include and ThenInclude expressions
/// on an <see cref="IQueryable{TEntity}"/> for Entity Framework queries.
/// </summary>
/// <typeparam name="TEntity">The type of the root entity.</typeparam>
internal sealed class IncludeChain<TEntity> where TEntity : EntityBase
{
    private readonly List<Func<IQueryable<TEntity>, IQueryable<TEntity>>> _includeChain = [];

    private Func<IQueryable<TEntity>, IQueryable<TEntity>>? _currentInclude;
    private object? _lastIncludable;

    /// <summary>
    /// Adds an Include operation to the chain for the specified navigation property.
    /// </summary>
    /// <typeparam name="TProperty">The type of the navigation property.</typeparam>
    /// <param name="navigation">An expression representing the navigation property to include.</param>
    /// <returns>The current <see cref="IncludeChain{TEntity}"/> instance.</returns>
    public IncludeChain<TEntity> AddInclude<TProperty>(
        Expression<Func<TEntity, TProperty>> navigation)
    {
        if (_currentInclude is not null)
        {
            _includeChain.Add(_currentInclude);
            _currentInclude = null;
        }

        _currentInclude = query =>
        {
            var result = query.Include(navigation);
            _lastIncludable = result;
            return result;
        };

        return this;
    }

    /// <summary>
    /// Adds a ThenInclude operation to the chain for a reference navigation property
    /// following a previous Include or ThenInclude.
    /// </summary>
    /// <typeparam name="TPreviousProperty">The type of the previous navigation property.</typeparam>
    /// <typeparam name="TProperty">The type of the current navigation property.</typeparam>
    /// <param name="navigation">An expression representing the navigation property to include.</param>
    /// <returns>The current <see cref="IncludeChain{TEntity}"/> instance.</returns>
    public IncludeChain<TEntity> AddThenInclude<TPreviousProperty, TProperty>(
        Expression<Func<TPreviousProperty, TProperty>> navigation)
    {
        if (_lastIncludable is null)
            throw new InvalidArgumentException("ThenInclude called before Include");

        var previousInclude = _currentInclude!;
        _currentInclude = query =>
        {
            var prevResult = previousInclude(query);
            var result = ((IIncludableQueryable<TEntity, TPreviousProperty>)prevResult).ThenInclude(navigation);
            _lastIncludable = result;
            return result;
        };

        return this;
    }

    /// <summary>
    /// Adds a ThenInclude operation to the chain for a collection navigation property
    /// following a previous Include or ThenInclude.
    /// </summary>
    /// <typeparam name="TPreviousProperty">The type of the previous navigation property.</typeparam>
    /// <typeparam name="TProperty">The type of the elements in the collection navigation property.</typeparam>
    /// <param name="navigation">An expression representing the collection navigation property to include.</param>
    /// <returns>The current <see cref="IncludeChain{TEntity}"/> instance.</returns>
    /// <exception cref="InvalidArgumentException">Thrown if called before an Include operation.</exception>
    public IncludeChain<TEntity> AddThenIncludeCollection<TPreviousProperty, TProperty>(
        Expression<Func<TPreviousProperty, IEnumerable<TProperty>>> navigation)
    {
        if (_lastIncludable is null)
            throw new InvalidArgumentException("ThenIncludeCollection called before Include.");

        var previousInclude = _currentInclude!;
        _currentInclude = query =>
        {
            var prevResult = previousInclude(query);
            var result = ((IIncludableQueryable<TEntity, TPreviousProperty>)prevResult).ThenInclude(navigation);
            _lastIncludable = result;
            return result;
        };

        return this;
    }

    /// <summary>
    /// Applies all configured Include and ThenInclude expressions to the given query.
    /// </summary>
    /// <param name="query">The <see cref="IQueryable{TEntity}"/> to apply includes to.</param>
    /// <returns>The modified query with all Include and ThenInclude operations applied.</returns>
    public IQueryable<TEntity> Apply(IQueryable<TEntity> query)
    {
        if (_currentInclude is not null)
        {
            _includeChain.Add(_currentInclude);
            _currentInclude = null;
        }

        foreach (var include in _includeChain)
            query = include(query);

        return query;
    }
}
