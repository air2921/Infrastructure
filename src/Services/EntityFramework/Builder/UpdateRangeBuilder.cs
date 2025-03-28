using Infrastructure.Services.EntityFramework.Entity;

namespace Infrastructure.Services.EntityFramework.Builder;

/// <summary>
/// Fluent builder for configuring bulk entity updates with optional audit tracking
/// </summary>
/// <typeparam name="TEntity">Type of entity to update, must inherit from EntityBase</typeparam>
public class UpdateRangeBuilder<TEntity> where TEntity : EntityBase
{
    private UpdateRangeBuilder()
    {
        
    }

    /// <summary>
    /// Collection of entities to be updated
    /// </summary>
    public IReadOnlyCollection<TEntity> Entities { get; private set; } = [];

    /// <summary>
    /// Identifier of the user who performed the update (for auditing)
    /// </summary>
    public string? UpdatedByUser { get; private set; }

    /// <summary>
    /// Creates a new builder instance
    /// </summary>
    public static UpdateRangeBuilder<TEntity> Create() => new();

    /// <summary>
    /// Sets the entities to be updated
    /// </summary>
    /// <param name="entities">Collection of entities</param>
    /// <returns>Current builder instance</returns>
    public UpdateRangeBuilder<TEntity> WithEntities(IEnumerable<TEntity> entities)
    {
        Entities = entities?.ToList() ?? throw new ArgumentNullException(nameof(entities));
        return this;
    }

    /// <summary>
    /// Sets a single entity to be updated
    /// </summary>
    /// <param name="entity">Entity to update</param>
    /// <returns>Current builder instance</returns>
    public UpdateRangeBuilder<TEntity> WithEntity(TEntity entity)
    {
        ArgumentNullException.ThrowIfNull(entity);
        Entities = [entity];
        return this;
    }

    /// <summary>
    /// Sets the user who performed the update
    /// </summary>
    /// <param name="user">User identifier/name</param>
    /// <returns>Current builder instance</returns>
    public UpdateRangeBuilder<TEntity> WithUpdatedBy(string? user)
    {
        UpdatedByUser = user;
        return this;
    }

    /// <summary>
    /// Validates the builder configuration
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when no entities are specified for update</exception>
    public void Validate()
    {
        if (Entities.Count == 0)
            throw new InvalidOperationException("No entities specified for update");
    }
}
