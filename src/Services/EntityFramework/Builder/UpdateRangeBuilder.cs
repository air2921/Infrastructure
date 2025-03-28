using Infrastructure.Services.EntityFramework.Entity;

namespace Infrastructure.Services.EntityFramework.Builder;

/// <summary>
/// A class that helps build parameters for updating a range of entities.
/// <para>This class provides a way to specify which entities should be updated and optionally track who performed the update.</para>
/// </summary>
/// <typeparam name="TEntity">The type of the entities to update.</typeparam>
public class UpdateRangeBuilder<TEntity> : EntityBase
{
    /// <summary>
    /// The collection of entities to be updated.
    /// <para>This collection contains the entity instances with their updated property values that need to be persisted.</para>
    /// </summary>
    public IEnumerable<TEntity> Entities { get; set; } = [];

    /// <summary>
    /// The identifier or name of the user who performed the update (optional).
    /// <para>This property can be used for audit purposes to track who made changes to the entities.</para>
    /// </summary>
    public string? UpdatedByUser { get; set; }
}
