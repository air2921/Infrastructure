using Infrastructure.Exceptions;
using Infrastructure.Services.EntityFramework.Entity;
using System.ComponentModel;

namespace Infrastructure.Services.EntityFramework.Builder.NoneQuery.Update;

/// <summary>
/// A class that helps build parameters for updating a single entity.
/// <para>This class provides a way to specify an entity to be updated and optionally track who performed the update.</para>
/// </summary>
/// <typeparam name="TEntity">The type of the entity to update.</typeparam>
public sealed class UpdateSingleBuilder<TEntity> : NoneQueryBuilder where TEntity : EntityBase
{
    /// <summary>
    /// Private constructor to enforce use of factory method.
    /// </summary>
    private UpdateSingleBuilder()
    {

    }

    /// <summary>
    /// The entity to be updated.
    /// <para>This property contains the entity instance with its updated property values that need to be persisted.</para>
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    internal TEntity Entity { get; private set; } = default!;

    /// <summary>
    /// The identifier or name of the user who performed the update (optional).
    /// <para>This property can be used for audit purposes to track who made changes to the entity.</para>
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    internal string? UpdatedByUser { get; private set; }

    /// <summary>
    /// Creates a new instance of the builder
    /// </summary>
    public static UpdateSingleBuilder<TEntity> Create() => new();

    /// <summary>
    /// Sets the entity to be updated
    /// </summary>
    /// <param name="entity">The entity to update</param>
    /// <returns>The current builder instance</returns>
    public UpdateSingleBuilder<TEntity> WithEntity(TEntity entity)
    {
        Entity = entity ?? throw new InvalidArgumentException("Entity for updating cannot be null");
        return this;
    }

    /// <summary>
    /// Sets the user who performed the update
    /// </summary>
    /// <param name="user">User identifier/name</param>
    /// <returns>The current builder instance</returns>
    public UpdateSingleBuilder<TEntity> WithUpdatedBy(string? user)
    {
        UpdatedByUser = user;
        return this;
    }
}
