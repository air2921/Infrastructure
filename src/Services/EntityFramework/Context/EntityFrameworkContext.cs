using Infrastructure.Services.EntityFramework.Entity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Query;
using System.Linq.Expressions;
using System.Reflection;

namespace Infrastructure.Services.EntityFramework.Context;

/// <summary>
/// Base database context with soft delete functionality and audit tracking
/// </summary>
/// <param name="options">The options to be used by a DbContext</param>
public abstract class EntityFrameworkContext(DbContextOptions options) : DbContext(options)
{
    /// <summary>
    /// Restores a soft-deleted entity by marking it as active
    /// </summary>
    /// <typeparam name="TEntity">Entity type that inherits from EntityBase</typeparam>
    /// <param name="entity">Entity to restore</param>
    /// <returns>The restored entity</returns>
    /// <exception cref="ArgumentNullException">Thrown when entity is null</exception>
    /// <remarks>
    /// This method will:
    /// 1. Set IsDeleted to false
    /// 2. Update UpdatedAt timestamp
    /// 3. Mark entity as Modified
    /// Note: Changes are not saved until SaveChanges is called
    /// </remarks>
    public virtual TEntity Restore<TEntity>(TEntity entity) where TEntity : EntityBase
    {
        ArgumentNullException.ThrowIfNull(entity);

        entity.IsDeleted = false;
        entity.UpdatedAt = DateTimeOffset.UtcNow;
        Entry(entity).State = EntityState.Modified;

        return entity;
    }

    /// <summary>
    /// Restores multiple soft-deleted entities in a batch operation
    /// </summary>
    /// <typeparam name="TEntity">Entity type that inherits from EntityBase</typeparam>
    /// <param name="entities">Collection of entities to restore</param>
    /// <returns>Collection of restored entities</returns>
    /// <exception cref="ArgumentNullException">Thrown when entities collection is null</exception>
    /// <remarks>
    /// Applies the same restoration logic as Restore&lt;TEntity&gt; to each entity in the collection.
    /// Changes are not saved until SaveChanges is called.
    /// </remarks>
    public virtual IEnumerable<TEntity> RestoreRange<TEntity>(IEnumerable<TEntity> entities) where TEntity : EntityBase
    {
        foreach (var entity in entities)
            Restore(entity);

        return entities;
    }

    /// <summary>
    /// Configures the model that was discovered by convention from the entity types
    /// </summary>
    /// <param name="modelBuilder">The builder being used to construct the model</param>
    /// <remarks>
    /// Applies configurations from the assembly and sets up soft-delete filtering
    /// for all entities inheriting from EntityBase.
    /// </remarks>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (typeof(EntityBase).IsAssignableFrom(entityType.ClrType))
            {
                modelBuilder.Entity(entityType.ClrType)
                    .HasQueryFilter(ConvertFilterExpression<EntityBase>(e => !e.IsDeleted, entityType.ClrType));
            }
        }
    }

    private static LambdaExpression ConvertFilterExpression<TEntity>(
        Expression<Func<TEntity, bool>> filterExpression,
        Type entityType)
    {
        var newParam = Expression.Parameter(entityType);
        var newBody = ReplacingExpressionVisitor.Replace(
            filterExpression.Parameters.Single(), newParam, filterExpression.Body);

        return Expression.Lambda(newBody, newParam);
    }

    /// <summary>
    /// Saves all changes made in this context to the database
    /// </summary>
    /// <param name="acceptAllChangesOnSuccess">Indicates whether AcceptAllChanges is called</param>
    /// <returns>The number of state entries written to the database</returns>
    public override int SaveChanges(bool acceptAllChangesOnSuccess)
    {
        OnBeforeSaving();
        return base.SaveChanges(acceptAllChangesOnSuccess);
    }

    /// <summary>
    /// Asynchronously saves all changes made in this context to the database
    /// </summary>
    /// <param name="acceptAllChangesOnSuccess">Indicates whether AcceptAllChanges is called</param>
    /// <param name="cancellationToken">A CancellationToken to observe while waiting for the task to complete</param>
    /// <returns>The number of state entries written to the database</returns>
    public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
    {
        OnBeforeSaving();
        return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
    }

    /// <summary>
    /// Saves all changes made in this context to the database
    /// </summary>
    /// <returns>The number of state entries written to the database</returns>
    public override int SaveChanges()
    {
        OnBeforeSaving();
        return base.SaveChanges();
    }

    /// <summary>
    /// Asynchronously saves all changes made in this context to the database
    /// </summary>
    /// <param name="cancellationToken">A CancellationToken to observe while waiting for the task to complete</param>
    /// <returns>The number of state entries written to the database</returns>
    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        OnBeforeSaving();
        return base.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// Applies audit tracking and delete behavior before saving changes to the database.
    /// </summary>
    /// <remarks>
    /// This method performs the following actions:
    /// - For <see cref="EntityState.Added"/>:
    ///   - Sets <c>CreatedAt</c> and <c>UpdatedAt</c> timestamps via <see cref="OnAdding"/>.
    /// - For <see cref="EntityState.Modified"/>:
    ///   - Updates <c>UpdatedAt</c> timestamp via <see cref="OnModifying"/>.
    /// - For <see cref="EntityState.Deleted"/>:
    ///   - If the entity inherits from <see cref="SoftEntityBase"/>:
    ///     - Performs a soft delete (sets <c>IsDeleted</c> to true and updates <c>UpdatedAt</c>) via <see cref="OnSoftRemoving"/>.
    ///   - If the entity inherits from <see cref="HardEntityBase"/>:
    ///     - Leaves the deletion as-is for physical removal via <see cref="OnHardRemoving"/>.
    ///   - Otherwise:
    ///     - Calls <see cref="OnRemoving"/> as fallback.
    /// </remarks>
    private void OnBeforeSaving()
    {
        var entries = ChangeTracker.Entries();

        foreach (var entry in entries)
        {
            if (entry.Entity is EntityBase entity)
            {
                switch (entry.State)
                {
                    case EntityState.Deleted when entity is SoftEntityBase softDeleteEntity:
                        OnSoftRemoving(entry, softDeleteEntity);
                        break;

                    case EntityState.Deleted when entity is HardEntityBase hardDeleteEntity:
                        OnHardRemoving(entry, hardDeleteEntity);
                        break;

                    case EntityState.Added:
                        OnAdding(entry, entity);
                        break;

                    case EntityState.Modified:
                        OnModifying(entry, entity);
                        break;

                    case EntityState.Deleted:
                        OnRemoving(entry, entity);
                        break;
                }
            }
        }
    }

    /// <summary>
    /// Applies audit fields for newly added entities.
    /// </summary>
    /// <param name="entry">The change tracker entry for the entity.</param>
    /// <param name="entity">The entity being added.</param>
    /// <remarks>
    /// Sets <c>CreatedAt</c> and <c>UpdatedAt</c> to the current UTC time.
    /// </remarks>
    protected virtual void OnAdding(EntityEntry entry, EntityBase entity)
    {
        entity.CreatedAt = DateTimeOffset.UtcNow;
        entity.UpdatedAt = DateTimeOffset.UtcNow;
    }

    /// <summary>
    /// Updates audit fields for modified entities.
    /// </summary>
    /// <param name="entry">The change tracker entry for the entity.</param>
    /// <param name="entity">The entity being modified.</param>
    /// <remarks>
    /// Updates <c>UpdatedAt</c> to the current UTC time.
    /// </remarks>
    protected virtual void OnModifying(EntityEntry entry, EntityBase entity)
    {
        entity.UpdatedAt = DateTimeOffset.UtcNow;
    }

    /// <summary>
    /// Fallback method invoked when an entity marked as deleted does not match <see cref="SoftEntityBase"/> or <see cref="HardEntityBase"/>.
    /// </summary>
    /// <param name="entry">The change tracker entry for the entity.</param>
    /// <param name="entity">The entity being removed.</param>
    /// <remarks>
    /// By default, does nothing. Can be overridden to apply custom delete logic for other <see cref="EntityBase"/> types.
    /// </remarks>
    protected virtual void OnRemoving(EntityEntry entry, EntityBase entity)
    {
        return;
    }

    /// <summary>
    /// Handles physical deletion of entities that inherit from <see cref="HardEntityBase"/>.
    /// </summary>
    /// <param name="entry">The change tracker entry for the entity.</param>
    /// <param name="entity">The hard-deletable entity being removed.</param>
    /// <remarks>
    /// By default, this method does nothing. Override if additional logic is needed during hard deletion.
    /// </remarks>
    protected virtual void OnHardRemoving(EntityEntry entry, HardEntityBase entity)
    {
        return;
    }

    /// <summary>
    /// Handles soft deletion of entities that inherit from <see cref="SoftEntityBase"/>.
    /// </summary>
    /// <param name="entry">The change tracker entry for the entity.</param>
    /// <param name="entity">The soft-deletable entity being removed.</param>
    /// <remarks>
    /// Sets <c>IsDeleted</c> to true, updates <c>UpdatedAt</c> timestamp, and converts the entity state to <see cref="EntityState.Modified"/>.
    /// </remarks>
    protected virtual void OnSoftRemoving(EntityEntry entry, SoftEntityBase entity)
    {
        entry.State = EntityState.Modified;
        entity.IsDeleted = true;
        entity.UpdatedAt = DateTimeOffset.UtcNow;
    }
}
