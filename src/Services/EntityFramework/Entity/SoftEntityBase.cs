namespace Infrastructure.Services.EntityFramework.Entity;

/// <summary>
/// Represents an entity that supports soft deletion (logical removal).
/// </summary>
/// <remarks>
/// Inherits all audit properties from <see cref="EntityBase"/>.
/// When marked as deleted, the entity will remain in the database with <c>IsDeleted = true</c>
/// and be excluded from queries by default via a global query filter.
/// </remarks>
public abstract class SoftEntityBase : EntityBase;
