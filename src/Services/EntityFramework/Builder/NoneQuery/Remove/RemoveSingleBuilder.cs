using Infrastructure.Enums;
using Infrastructure.Exceptions;
using Infrastructure.Services.EntityFramework.Entity;
using System.ComponentModel;
using System.Linq.Expressions;

namespace Infrastructure.Services.EntityFramework.Builder.NoneQuery.Remove;

/// <summary>
/// A builder class for configuring parameters to remove a single entity.
/// Provides multiple ways to specify entity for removal: by instance, identifier, or filter expression.
/// </summary>
/// <typeparam name="TEntity">The type of entity to remove, must inherit from EntityBase.</typeparam>
public sealed class RemoveSingleBuilder<TEntity> : NoneQueryBuilder where TEntity : EntityBase
{
    /// <summary>
    /// Private constructor to enforce use of factory method.
    /// </summary>
    private RemoveSingleBuilder()
    {
    }

    /// <summary>
    /// Identifier of the entity to be removed.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    internal string? Id { get; private set; }

    /// <summary>
    /// Entity instance to be removed.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    internal TEntity? Entity { get; private set; }

    /// <summary>
    /// Filter expression to select single entity for removal.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    internal Expression<Func<TEntity, bool>>? Filter { get; private set; }

    /// <summary>
    /// Specifies the removal mode (by entity, identifier or filter).
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    internal RemoveByMode RemoveByMode { get; private set; }

    /// <summary>
    /// Creates a new instance of RemoveSingleBuilder.
    /// </summary>
    /// <returns>New instance of RemoveSingleBuilder</returns>
    public static RemoveSingleBuilder<TEntity> Create() => new();

    /// <summary>
    /// Sets the entity instance to be removed.
    /// </summary>
    /// <param name="entity">Entity instance to remove.</param>
    /// <returns>The current builder instance.</returns>
    public RemoveSingleBuilder<TEntity> WithEntity(TEntity entity)
    {
        Entity = entity ?? throw new InvalidArgumentException("Entity for remove cannot be null");
        RemoveByMode = RemoveByMode.Entity;
        return this;
    }

    /// <summary>
    /// Sets the identifier of entity to be removed.
    /// </summary>
    /// <param name="identifier">Entity identifier.</param>
    /// <returns>The current builder instance.</returns>
    public RemoveSingleBuilder<TEntity> WithIdentifier(string identifier)
    {
        Id = identifier ?? throw new InvalidArgumentException("Identifier of entity for remove cannot be null");
        RemoveByMode = RemoveByMode.Identifier;
        return this;
    }

    /// <summary>
    /// Sets the filter expression to select single entity for removal.
    /// </summary>
    /// <param name="filter">Filter expression to select entity.</param>
    /// <returns>The current builder instance.</returns>
    public RemoveSingleBuilder<TEntity> WithFilter(Expression<Func<TEntity, bool>> filter)
    {
        Filter = filter ?? throw new InvalidArgumentException("Filter for filtering entity cannot be null");
        RemoveByMode = RemoveByMode.Filter;
        return this;
    }

    /// <summary>
    /// Explicitly sets the removal mode.
    /// </summary>
    /// <param name="mode">The removal mode to use.</param>
    /// <returns>The current builder instance.</returns>
    public RemoveSingleBuilder<TEntity> WithRemoveMode(RemoveByMode mode)
    {
        RemoveByMode = mode;
        return this;
    }
}
