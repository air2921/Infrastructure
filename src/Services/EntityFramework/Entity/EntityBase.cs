using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Infrastructure.Services.EntityFramework.Entity;

/// <summary>
/// Represents a base class for entities with common audit and domain properties.
/// <para>Provides default identifiers and audit tracking for entities.</para>
/// </summary>
[Index(nameof(IsDeleted), IsUnique = false)]
[Index(nameof(PublicId), IsUnique = true)]
public abstract class EntityBase
{
    /// <summary>
    /// Gets or sets the unique identifier for the entity.
    /// <para>The identifier is automatically initialized to a new GUID as string if not set.</para>
    /// </summary>
    [Key]
    [MaxLength(128)]
    public virtual string Id { get; set; } = Guid.NewGuid().ToString();

    /// <summary>
    /// Gets or sets the public-facing identifier for the entity.
    /// <para>
    /// This value is automatically generated from the database sequence "PublicIdSequence"
    /// starting at 10000 and increments by 1. Provides a more user-friendly identifier
    /// than the GUID-based Id property.
    /// </para>
    /// <para>
    /// The PublicId is unique across all entities of the same type (enforced by database index).
    /// </para>
    /// </summary>
    public virtual int PublicId { get; set; }

    /// <summary>
    /// Gets or sets the date and time when the entity was created, in UTC format.
    /// </summary>
    [Column]
    public virtual DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    /// <summary>
    /// Gets or sets the date and time when the entity was last modified, in UTC format.
    /// <para>Null if the entity has never been modified.</para>
    /// </summary>
    [Column]
    public virtual DateTimeOffset? UpdatedAt { get; set; }

    /// <summary>
    /// Gets or sets the identifier of the user who created the entity.
    /// </summary>
    [Column]
    [MaxLength(128)]
    public virtual string? CreatedBy { get; set; }

    /// <summary>
    /// Gets or sets the identifier of the user who last modified the entity.
    /// </summary>
    [Column]
    [MaxLength(128)]
    public virtual string? UpdatedBy { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the entity is logically deleted.
    /// <para>Implements soft delete pattern when true.</para>
    /// </summary>
    [Column]
    public virtual bool IsDeleted { get; set; } = false;
}
