namespace Infrastructure.Services.EntityFramework.Entity;

/// <summary>
/// Represents an entity that supports physical (hard) deletion from the database.
/// </summary>
/// <remarks>
/// Inherits all audit properties from <see cref="EntityBase"/> but does not override deletion behavior.
/// When marked as deleted, the entity will be physically removed from the database.
/// </remarks>
public abstract class HardEntityBase : EntityBase;
