using Infrastructure.Exceptions;
using Infrastructure.Services.EntityFramework.Entity;
using System.ComponentModel;

namespace Infrastructure.Services.EntityFramework.Builder.NoneQuery.Create;

/// <summary>
/// Fluent builder for configuring single entity creation with optional audit tracking
/// </summary>
/// <typeparam name="TEntity">Type of entity to create, must inherit from EntityBase</typeparam>
public sealed class CreateSingleBuilder<TEntity> : NoneQueryBuilder where TEntity : EntityBase
{
    /// <summary>
    /// Private constructor to enforce use of factory method.
    /// </summary>
    private CreateSingleBuilder()
    {

    }

    /// <summary>
    /// Entity to be created
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    internal TEntity Entity { get; private set; } = default!;

    /// <summary>
    /// Identifier of the user who performed the creation (for auditing)
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    internal string? CreatedByUser { get; private set; }

    /// <summary>
    /// Creates a new builder instance
    /// </summary>
    /// <returns>New instance of CreateSingleBuilder</returns>
    public static CreateSingleBuilder<TEntity> Create() => new();

    /// <summary>
    /// Sets the entity to be created
    /// </summary>
    /// <param name="entity">Entity instance</param>
    /// <returns>Current builder instance</returns>
    public CreateSingleBuilder<TEntity> WithEntity(TEntity entity)
    {
        Entity = entity ?? throw new InvalidArgumentException("Entity for creation cannot be null");
        return this;
    }

    /// <summary>
    /// Sets the user who performed the creation
    /// </summary>
    /// <param name="user">User identifier/name</param>
    /// <returns>Current builder instance</returns>
    public CreateSingleBuilder<TEntity> WithCreatedBy(string? user)
    {
        CreatedByUser = user;
        return this;
    }
}
