using Infrastructure.Enums;
using Infrastructure.Services.EntityFramework.Entity;

namespace Infrastructure.Services.EntityFramework.Builder;

/// <summary>
/// A class that helps build parameters for removing a range of entities.
/// <para>This class provides flexible ways to specify which entities should be removed - either by providing the entities directly or their identifiers.</para>
/// </summary>
/// <typeparam name="TEntity">The type of the entities to remove.</typeparam>
public class RemoveRangeBuilder<TEntity> : EntityBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="RemoveRangeBuilder{TEntity}"/> class.
    /// <para>The constructor automatically sets the <see cref="RemoveBy"/> mode based on which collections (<see cref="Entities"/> or <see cref="Identifiers"/>) are populated.</para>
    /// </summary>
    public RemoveRangeBuilder()
    {
        RemoveByMode = GetDefaultRemoveByMode();
    }

    /// <summary>
    /// The collection of entities to be removed.
    /// <para>This collection is used when the removal should be performed using entity instances directly.</para>
    /// </summary>
    public IEnumerable<TEntity> Entities { get; set; } = [];

    /// <summary>
    /// The collection of identifiers for entities to be removed.
    /// <para>This collection is used when the removal should be performed using entity identifiers rather than full entity instances.</para>
    /// </summary>
    public IEnumerable<object> Identifiers { get; set; } = [];

    /// <summary>
    /// Specifies the mode by which entities should be removed.
    /// <para>Determines whether the removal will use <see cref="Entities"/> or <see cref="Identifiers"/>.</para>
    /// </summary>
    public RemoveByMode RemoveByMode { get; set; }

    /// <summary>
    /// Determines the default removal mode based on which collections are populated.
    /// <para>If <see cref="Entities"/> contains items, defaults to <see cref="RemoveByMode.Entities"/>.
    /// If <see cref="Identifiers"/> contains items, defaults to <see cref="RemoveByMode.Identifiers"/>.
    /// Otherwise defaults to <see cref="RemoveByMode.Entities"/>.</para>
    /// </summary>
    /// <returns>The determined <see cref="RemoveByMode"/>.</returns>
    private RemoveByMode GetDefaultRemoveByMode()
    {
        bool hasEntities = Entities.Any();
        bool hasIdentifiers = Identifiers.Any();

        if (hasEntities)
            return RemoveByMode.Entities;

        if (hasIdentifiers)
            return RemoveByMode.Identifiers;

        return RemoveByMode.Entities;
    }
}
