using Infrastructure.Services.EntityFramework.Entity;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Infrastructure.Exceptions;

namespace Infrastructure.Services.EntityFramework.Builder.Modules;

internal sealed class IncludeChain<TEntity> where TEntity : EntityBase
{
    private readonly List<Func<IQueryable<TEntity>, IQueryable<TEntity>>> _includeChain = [];

    private Func<IQueryable<TEntity>, IQueryable<TEntity>>? _currentInclude;
    private object? _lastIncludable;

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
