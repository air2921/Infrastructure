using Infrastructure.Services.EntityFramework.Entity;

namespace Infrastructure.Services.EntityFramework.Builder;

/// <summary>
/// A class that helps build parameters for updating a single entity.
/// <para>This class provides a way to specify an entity to be updated and optionally track who performed the update.</para>
/// </summary>
/// <typeparam name="TEntity">The type of the entity to update.</typeparam>
public class UpdateSingleBuilder<TEntity> : EntityBase
{
    /// <summary>
    /// The entity to be updated.
    /// <para>This property contains the entity instance with its updated property values that need to be persisted.</para>
    /// </summary>
    public TEntity Entity { get; set; } = default!;

    /// <summary>
    /// The identifier or name of the user who performed the update (optional).
    /// <para>This property can be used for audit purposes to track who made changes to the entity.</para>
    /// </summary>
    public string? UpdatedByUser { get; set; }
}
