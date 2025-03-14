using System.ComponentModel.DataAnnotations;

namespace Infrastructure.Services.EntityFramework.Entity;

/// <summary>
/// Represents a base class for entities with an identifier.
/// <para>Provides a default identifier for the entity, which is of type <see cref="string"/> and is initialized as a new GUID.</para>
/// </summary>
public abstract class EntityBase
{
    /// <summary>
    /// Gets or sets the unique identifier for the entity.
    /// <para>The identifier is automatically initialized to a new GUID as string if not set.</para>
    /// </summary>
    [Key]
    public virtual string Id { get; set; } = Guid.NewGuid().ToString();
}
