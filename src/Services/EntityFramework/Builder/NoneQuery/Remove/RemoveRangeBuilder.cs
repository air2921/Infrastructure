using Infrastructure.Enums;
using Infrastructure.Exceptions;
using Infrastructure.Services.EntityFramework.Builder.Base;
using Infrastructure.Services.EntityFramework.Entity;
using System.ComponentModel;
using System.Linq.Expressions;

namespace Infrastructure.Services.EntityFramework.Builder.NoneQuery.Remove;

/// <summary>
/// A builder class for configuring parameters to remove a range of entities.
/// Provides flexible ways to specify entities for removal either by entity instances or their identifiers.
/// </summary>
/// <typeparam name="TEntity">The type of entities to remove, must inherit from EntityBase.</typeparam>
public sealed class RemoveRangeBuilder<TEntity> : NoneQueryBuilder<RemoveRangeBuilder<TEntity>, TEntity> where TEntity : EntityBase
{
    /// <summary>
    /// Private constructor to enforce use of factory method.
    /// </summary>
    private RemoveRangeBuilder()
    {
    }

    /// <summary>
    /// Collection of entities to be removed directly.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    internal IEnumerable<TEntity> Entities { get; private set; } = [];

    /// <summary>
    /// Collection of entity identifiers for entities to be removed.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    internal IEnumerable<object> Identifiers { get; private set; } = [];

    /// <summary>
    /// Filter expression to select entities for removal.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    internal Expression<Func<TEntity, bool>>? Filter { get; private set; }

    /// <summary>
    /// Specifies the removal mode (by entities, identifiers or filter).
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    internal RemoveByMode RemoveByMode { get; private set; }

    /// <summary>
    /// Creates a new instance of RemoveRangeBuilder.
    /// </summary>
    /// <returns>New instance of RemoveRangeBuilder</returns>
    public static RemoveRangeBuilder<TEntity> Create() => new();

    /// <summary>
    /// Sets the entities to be removed directly.
    /// </summary>
    /// <param name="entities">Collection of entities to remove.</param>
    /// <returns>The current builder instance.</returns>
    public RemoveRangeBuilder<TEntity> WithEntities(IEnumerable<TEntity> entities)
    {
        Entities = entities.ToArray();
        RemoveByMode = RemoveByMode.Entity;
        return this;
    }

    /// <summary>
    /// Sets the identifiers of entities to be removed.
    /// </summary>
    /// <param name="identifiers">Collection of entity identifiers.</param>
    /// <returns>The current builder instance.</returns>
    public RemoveRangeBuilder<TEntity> WithIdentifiers(IEnumerable<string> identifiers)
    {
        Identifiers = identifiers.ToArray();
        RemoveByMode = RemoveByMode.Identifier;
        return this;
    }

    /// <summary>
    /// Sets the filter expression to select entities for removal.
    /// </summary>
    /// <param name="filter">Filter expression to select entities.</param>
    /// <returns>The current builder instance.</returns>
    public RemoveRangeBuilder<TEntity> WithFilter(Expression<Func<TEntity, bool>> filter)
    {
        Filter = filter ?? throw new InvalidArgumentException("Filter for filtering entities cannot be null");
        RemoveByMode = RemoveByMode.Filter;
        return this;
    }

    /// <summary>
    /// Explicitly sets the removal mode.
    /// </summary>
    /// <param name="mode">The removal mode to use.</param>
    /// <returns>The current builder instance.</returns>
    public RemoveRangeBuilder<TEntity> WithRemoveMode(RemoveByMode mode)
    {
        RemoveByMode = mode;
        return this;
    }
}
