namespace Infrastructure.Enums;

/// <summary>
/// Specifies the mode for removing entities from a collection or repository.
/// <b>Use carefully as different modes may have different performance implications.</b>
/// </summary>
public enum RemoveByMode
{
    /// <summary>
    /// Remove entity by unique identifier (ID = 101).
    /// <para>
    /// This mode is efficient for bulk operations where you only have the ID of entity.
    /// Typically used when you want to avoid loading full entities into memory.
    /// </para>
    /// </summary>
    Identifier = 101,

    /// <summary>
    /// Remove actual entity object (ID = 201).
    /// <para>
    /// This mode is useful when you already have loaded entity and want to ensure
    /// proper tracking and cascade deletion if configured in the data model.
    /// </para>
    /// </summary>
    Entity = 201,

    /// <summary>
    /// Remove entity by filter (ID = 301).
    /// <para>
    /// This mode is effective for bulk operations where you don't have an entity or entity ID.
    /// Typically used when you want to avoid loading full entities into memory.
    /// </para>
    /// </summary>
    Filter = 301
}
