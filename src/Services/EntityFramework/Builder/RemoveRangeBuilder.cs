using Infrastructure.Enums;
using Infrastructure.Services.EntityFramework.Entity;

namespace Infrastructure.Services.EntityFramework.Builder;

/// <summary>
/// A builder class for configuring parameters to remove a range of entities.
/// Provides flexible ways to specify entities for removal either by entity instances or their identifiers.
/// </summary>
/// <typeparam name="TEntity">The type of entities to remove, must inherit from EntityBase.</typeparam>
public class RemoveRangeBuilder<TEntity> : EntityBase where TEntity : EntityBase
{
    /// <summary>
    /// Collection of entities to be removed directly.
    /// </summary>
    public IEnumerable<TEntity> Entities { get; private set; } = [];

    /// <summary>
    /// Collection of entity identifiers for entities to be removed.
    /// </summary>
    public IEnumerable<object> Identifiers { get; private set; } = [];

    /// <summary>
    /// Specifies the removal mode (by entities or identifiers).
    /// </summary>
    public RemoveByMode RemoveByMode { get; private set; }

    /// <summary>
    /// Creates a new instance of RemoveRangeBuilder.
    /// </summary>
    public static RemoveRangeBuilder<TEntity> Create() => new();

    /// <summary>
    /// Sets the entities to be removed directly.
    /// </summary>
    /// <param name="entities">Collection of entities to remove.</param>
    /// <returns>The current builder instance.</returns>
    public RemoveRangeBuilder<TEntity> WithEntities(IEnumerable<TEntity> entities)
    {
        Entities = entities?.ToList() ?? throw new ArgumentNullException(nameof(entities));
        RemoveByMode = RemoveByMode.Entities;
        return this;
    }

    /// <summary>
    /// Sets a single entity to be removed.
    /// </summary>
    /// <param name="entity">The entity to remove.</param>
    /// <returns>The current builder instance.</returns>
    public RemoveRangeBuilder<TEntity> WithEntity(TEntity entity)
    {
        ArgumentNullException.ThrowIfNull(entity);
        Entities = [entity];
        RemoveByMode = RemoveByMode.Entities;
        return this;
    }

    /// <summary>
    /// Sets the identifiers of entities to be removed.
    /// </summary>
    /// <param name="identifiers">Collection of entity identifiers.</param>
    /// <returns>The current builder instance.</returns>
    public RemoveRangeBuilder<TEntity> WithIdentifiers(IEnumerable<object> identifiers)
    {
        Identifiers = identifiers?.ToList() ?? throw new ArgumentNullException(nameof(identifiers));
        RemoveByMode = RemoveByMode.Identifiers;
        return this;
    }

    /// <summary>
    /// Sets a single entity identifier to be removed.
    /// </summary>
    /// <param name="identifier">The entity identifier to remove.</param>
    /// <returns>The current builder instance.</returns>
    public RemoveRangeBuilder<TEntity> WithIdentifier(object identifier)
    {
        ArgumentNullException.ThrowIfNull(identifier);
        Identifiers = [identifier];
        RemoveByMode = RemoveByMode.Identifiers;
        return this;
    }

    /// <summary>
    /// Explicitly sets the removal mode.
    /// </summary>
    /// <param name="mode">The removal mode to use.</param>
    /// <returns>The current builder instance.</returns>
    public RemoveRangeBuilder<TEntity> WithRemoveMode(RemoveByMode mode)
    {
        if (!Enum.IsDefined(typeof(RemoveByMode), mode))
            throw new ArgumentOutOfRangeException(nameof(mode));

        RemoveByMode = mode;
        return this;
    }

    /// <summary>
    /// Validates the builder configuration.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when configuration is invalid.</exception>
    public void Validate()
    {
        if (RemoveByMode == RemoveByMode.Entities && !Entities.Any())
            throw new InvalidOperationException("No entities specified for Entities removal mode");

        if (RemoveByMode == RemoveByMode.Identifiers && !Identifiers.Any())
            throw new InvalidOperationException("No identifiers specified for Identifiers removal mode");
    }
}
