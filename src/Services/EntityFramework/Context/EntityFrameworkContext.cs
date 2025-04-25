using Infrastructure.Abstractions.Database;
using Infrastructure.Exceptions;
using Infrastructure.Services.EntityFramework.Entity;
using Microsoft.EntityFrameworkCore;
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
    public TEntity Restore<TEntity>(TEntity entity) where TEntity : EntityBase
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
    public IEnumerable<TEntity> RestoreRange<TEntity>(IEnumerable<TEntity> entities) where TEntity : EntityBase
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
    /// Applies audit tracking (timestamps) before saving changes
    /// </summary>
    /// <remarks>
    /// Automatically:
    /// - Sets CreatedAt/UpdatedAt for new entities
    /// - Updates UpdatedAt for modified entities
    /// - Converts hard deletes to soft deletes for EntityBase types
    /// </remarks>
    private void OnBeforeSaving()
    {
        var entries = ChangeTracker.Entries();
        var utcNow = DateTimeOffset.UtcNow;

        foreach (var entry in entries)
        {
            if (entry.Entity is EntityBase entity)
            {
                switch (entry.State)
                {
                    case EntityState.Added:
                        entity.CreatedAt = utcNow;
                        entity.UpdatedAt = utcNow;
                        break;

                    case EntityState.Modified:
                        entity.UpdatedAt = utcNow;
                        break;

                    case EntityState.Deleted when entity is EntityBase softDeleteEntity:
                        entry.State = EntityState.Modified;
                        softDeleteEntity.IsDeleted = true;
                        softDeleteEntity.UpdatedAt = utcNow;
                        break;
                }
            }
        }
    }
}
