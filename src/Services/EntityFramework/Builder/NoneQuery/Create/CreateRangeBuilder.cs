using Infrastructure.Services.EntityFramework.Builder.NoneQuery.Remove;
using Infrastructure.Services.EntityFramework.Entity;
using System.ComponentModel;

namespace Infrastructure.Services.EntityFramework.Builder.NoneQuery.Create;

/// <summary>
/// Fluent builder for configuring bulk entity creation with optional audit tracking
/// </summary>
/// <typeparam name="TEntity">Type of entity to create, must inherit from EntityBase</typeparam>
public sealed class CreateRangeBuilder<TEntity> : NoneQueryBuilder<CreateRangeBuilder<TEntity>, TEntity> where TEntity : EntityBase
{
    /// <summary>
    /// Private constructor to enforce use of factory method.
    /// </summary>
    private CreateRangeBuilder()
    {

    }

    /// <summary>
    /// Collection of entities to be created
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    internal IReadOnlyCollection<TEntity> Entities { get; private set; } = [];

    /// <summary>
    /// Identifier of the user who performed the creation (for auditing)
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    internal string? CreatedByUser { get; private set; }

    /// <summary>
    /// Creates a new builder instance
    /// </summary>
    public static CreateRangeBuilder<TEntity> Create() => new();

    /// <summary>
    /// Sets the entities to be created
    /// </summary>
    /// <param name="entities">Collection of entities</param>
    /// <returns>The current builder instance.</returns>
    public CreateRangeBuilder<TEntity> WithEntities(IEnumerable<TEntity> entities)
    {
        Entities = entities.ToArray();
        return this;
    }

    /// <summary>
    /// Sets the user who performed the creation
    /// </summary>
    /// <param name="user">User identifier/name</param>
    /// <returns>The current builder instance.</returns>
    public CreateRangeBuilder<TEntity> WithCreatedBy(string? user)
    {
        CreatedByUser = user;
        return this;
    }
}
