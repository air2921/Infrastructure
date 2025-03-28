namespace Infrastructure.Enums;

/// <summary>
/// Specifies the mode for removing entities from a collection or repository.
/// <b>Use carefully as different modes may have different performance implications.</b>
/// </summary>
public enum RemoveByMode
{
    /// <summary>
    /// Remove entities by their unique identifiers (ID = 101).
    /// <para>
    /// This mode is efficient for bulk operations where you only have the IDs of entities.
    /// Typically used when you want to avoid loading full entities into memory.
    /// </para>
    /// </summary>
    Identifiers = 101,

    /// <summary>
    /// Remove actual entity objects (ID = 201).
    /// <para>
    /// This mode is useful when you already have loaded entities and want to ensure
    /// proper tracking and cascade deletion if configured in the data model.
    /// </para>
    /// </summary>
    Entities = 201
}
