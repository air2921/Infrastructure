using Infrastructure.Exceptions;
using Infrastructure.Services.EntityFramework.Builder;
using Infrastructure.Services.EntityFramework.Entity;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Infrastructure.Abstractions.Database;

/// <summary>
/// Represents a generic repository pattern for performing CRUD operations on entities of type <typeparamref name="TEntity"/>.
/// <para>All methods are thread-safe and support cancellation.</para>
/// </summary>
/// <typeparam name="TEntity">The type of the entity. It must inherit from <see cref="EntityBase"/>.</typeparam>
public interface IRepository<TEntity> where TEntity : EntityBase
{
    /// <summary>
    /// Returns an <see cref="IQueryable{TEntity}"/> to perform further queries on the entity set.
    /// <para>Thread-safe method.</para>
    /// </summary>
    /// <returns>An <see cref="IQueryable{TEntity}"/> representing the entity set.</returns>
    IQueryable<TEntity> GetQuery();

    /// <summary>
    /// Asynchronously retrieves the count of entities that match the specified filter.
    /// <para>Thread-safe method.</para>
    /// </summary>
    /// <param name="filter">The filter expression to apply to the entity set.</param>
    /// <returns>The count of entities that match the filter.</returns>
    /// <exception cref="EntityException">Thrown when an error occurs during the count operation.</exception>
    ValueTask<int> GetCountAsync(Expression<Func<TEntity, bool>>? filter);

    /// <summary>
    /// Asynchronously retrieves an entity by its identifier.
    /// <para>Thread-safe method.</para>
    /// </summary>
    /// <param name="id">The identifier of the entity to retrieve.</param>
    /// <param name="cancellationToken">A token to cancel the operation if needed.</param>
    /// <returns>The entity with the specified identifier, or <c>null</c> if not found.</returns>
    /// <exception cref="EntityException">Thrown when an error occurs during the retrieval operation.</exception>
    Task<TEntity?> GetByIdAsync(object id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Asynchronously retrieves the first or last entity that matches the specified filter, including options for ordering and tracking.
    /// <para>Thread-safe method.</para>
    /// </summary>
    /// <param name="builder">A <see cref="SingleQueryBuilder{TEntity}"/> that defines the query criteria.</param>
    /// <param name="cancellationToken">A token to cancel the operation if needed.</param>
    /// <returns>The first or last entity that matches the filter, or <c>null</c> if not found.</returns>
    /// <exception cref="EntityException">Thrown when an error occurs during the retrieval operation.</exception>
    Task<TEntity?> GetByFilterAsync(SingleQueryBuilder<TEntity> builder, CancellationToken cancellationToken = default);

    /// <summary>
    /// Asynchronously retrieves a range of entities based on the specified query builder.
    /// <para>Thread-safe method.</para>
    /// </summary>
    /// <param name="builder">A <see cref="RangeQueryBuilder{TEntity}"/> that defines the query criteria.</param>
    /// <param name="cancellationToken">A token to cancel the operation if needed.</param>
    /// <returns>A list of entities matching the query criteria.</returns>
    /// <exception cref="EntityException">Thrown when an error occurs during the retrieval operation.</exception>
    Task<IEnumerable<TEntity>> GetRangeAsync(RangeQueryBuilder<TEntity>? builder, CancellationToken cancellationToken = default);

    /// <summary>
    /// Asynchronously adds a new entity to the repository.
    /// <para>Thread-safe method.</para>
    /// </summary>
    /// <param name="entity">The entity to add.</param>
    /// <param name="cancellationToken">A token to cancel the operation if needed.</param>
    /// <returns>The added entity.</returns>
    /// <exception cref="EntityException">Thrown when an error occurs during the add operation.</exception>
    Task<TEntity> AddAsync(TEntity entity, CancellationToken cancellationToken = default);

    /// <summary>
    /// Asynchronously adds multiple entities to the repository.
    /// <para>Thread-safe method.</para>
    /// </summary>
    /// <param name="entities">The entities to add.</param>
    /// <param name="cancellationToken">A token to cancel the operation if needed.</param>
    /// <exception cref="EntityException">Thrown when an error occurs during the add operation.</exception>
    Task AddRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default);

    /// <summary>
    /// Asynchronously deletes an entity by its identifier.
    /// <para>Thread-safe method.</para>
    /// </summary>
    /// <param name="id">The identifier of the entity to delete.</param>
    /// <param name="cancellationToken">A token to cancel the operation if needed.</param>
    /// <returns>The deleted entity, or <c>null</c> if no entity was found with the specified identifier.</returns>
    /// <exception cref="EntityException">Thrown when an error occurs during the delete operation.</exception>
    Task<TEntity?> DeleteByIdAsync(object id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Asynchronously deletes an entity based on a specified filter.
    /// <para>Thread-safe method.</para>
    /// </summary>
    /// <param name="filter">An expression that defines the filter to identify the entity to delete.</param>
    /// <param name="cancellationToken">A token to cancel the operation if needed.</param>
    /// <returns>The deleted entity, or <c>null</c> if no entity matches the filter.</returns>
    /// <exception cref="EntityException">Thrown when an error occurs during the delete operation.</exception>
    Task<TEntity?> DeleteByFilterAsync(Expression<Func<TEntity, bool>> filter, CancellationToken cancellationToken = default);

    /// <summary>
    /// Asynchronously deletes a range of entities based on the specified builder parameters.
    /// <para>Thread-safe method.</para>
    /// </summary>
    /// <param name="builder">The builder containing entities or identifiers to delete and removal mode.</param>
    /// <param name="cancellationToken">A token to cancel the operation if needed.</param>
    /// <returns>The collection of deleted entities.</returns>
    /// <exception cref="EntityException">Thrown when an error occurs during the delete operation.</exception>
    Task<IEnumerable<TEntity>> DeleteRangeAsync(RemoveRangeBuilder<TEntity> builder, CancellationToken cancellationToken = default);

    /// <summary>
    /// Asynchronously updates a single entity based on the specified builder parameters.
    /// <para>Thread-safe method that automatically sets update tracking fields.</para>
    /// </summary>
    /// <param name="builder">The builder containing the entity to update and optional user information.</param>
    /// <param name="cancellationToken">A token to cancel the operation if needed.</param>
    /// <returns>The updated entity, or <c>null</c> if the update failed.</returns>
    /// <exception cref="EntityException">Thrown when an error occurs during the update operation.</exception>
    Task<TEntity> UpdateAsync(UpdateSingleBuilder<TEntity> builder, CancellationToken cancellationToken = default);

    /// <summary>
    /// Asynchronously updates a collection of entities based on the specified builder parameters.
    /// <para>Thread-safe method that automatically sets update tracking fields for all entities.</para>
    /// </summary>
    /// <param name="builder">The builder containing entities to update and optional user information.</param>
    /// <param name="cancellationToken">A token to cancel the operation if needed.</param>
    /// <returns>A collection of updated entities.</returns>
    /// <exception cref="EntityException">Thrown when an error occurs during the update operation.</exception>
    Task<IEnumerable<TEntity?>> UpdateRangeAsync(UpdateRangeBuilder<TEntity> builder, CancellationToken cancellationToken = default);

    /// <summary>
    /// Asynchronously restores a soft-deleted entity in the repository.
    /// <para>Sets IsDeleted to false and updates the UpdatedAt timestamp.</para>
    /// <para>Thread-safe method with write lock protection.</para>
    /// </summary>
    /// <param name="entity">The entity to restore. Must not be null.</param>
    /// <param name="cancellationToken">A token to cancel the operation if needed.</param>
    /// <returns>The restored entity with updated properties.</returns>
    /// <exception cref="EntityException">Thrown when an error occurs during the restore operation.</exception>
    /// <remarks>
    /// </remarks>
    public Task<TEntity> RestoreAsync(TEntity entity, CancellationToken cancellationToken = default);

    /// <summary>
    /// Asynchronously restores multiple soft-deleted entities in the repository.
    /// <para>Batch operation that efficiently restores all provided entities.</para>
    /// <para>Thread-safe method with write lock protection.</para>
    /// </summary>
    /// <param name="entities">The collection of entities to restore. Must not be null or contain null elements.</param>
    /// <param name="cancellationToken">A token to cancel the operation if needed.</param>
    /// <returns>The collection of restored entities with updated properties.</returns>
    /// <exception cref="EntityException">Thrown when an error occurs during the restore operation.</exception>
    /// <remarks>
    /// All changes are persisted in a single database transaction.
    /// </remarks>
    public Task<IEnumerable<TEntity>> RestoreRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default);
}

/// <summary>
/// Represents a generic repository pattern with support for a specific <typeparamref name="TDbContext"/> 
/// for performing CRUD operations on entities of type <typeparamref name="TEntity"/>.
/// </summary>
/// <typeparam name="TEntity">The type of the entity. It must inherit from <see cref="EntityBase"/>.</typeparam>
/// <typeparam name="TDbContext">The type of the database context. It must inherit from <see cref="DbContext"/>.</typeparam>
public interface IRepository<TEntity, TDbContext> : IRepository<TEntity> where TEntity : EntityBase where TDbContext : DbContext
{
}