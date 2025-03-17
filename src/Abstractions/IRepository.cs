using Infrastructure.Exceptions;
using Infrastructure.Services.EntityFramework.Builder;
using Infrastructure.Services.EntityFramework.Entity;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Infrastructure.Abstractions;

/// <summary>
/// Represents a generic repository pattern for performing CRUD operations on entities of type <typeparamref name="TEntity"/>.
/// </summary>
/// <typeparam name="TEntity">The type of the entity. It must inherit from <see cref="EntityBase"/>.</typeparam>
public interface IRepository<TEntity> where TEntity : EntityBase
{
    /// <summary>
    /// Gets a queryable collection of entities of type <typeparamref name="TEntity"/>.
    /// </summary>
    /// <returns>An <see cref="IQueryable{TEntity}"/> for querying the entities.</returns>
    /// <exception cref="EntityException">Thrown when an error occurs while fetching the entities.</exception>
    public IQueryable<TEntity> GetQuery();

    /// <summary>
    /// Gets the count of entities that match the provided filter asynchronously.
    /// </summary>
    /// <param name="filter">The filter expression to apply to the entities.</param>
    /// <returns>The total count of entities matching the filter expression as a <see cref="ValueTask{int}"/>.</returns>
    /// <exception cref="EntityException">Thrown when an error occurs while counting the entities.</exception>
    public ValueTask<int> GetCountAsync(Expression<Func<TEntity, bool>>? filter);

    /// <summary>
    /// Retrieves a range of entities based on the provided builder configuration.
    /// </summary>
    /// <param name="builder">The builder that specifies range-related query parameters.</param>
    /// <param name="cancellationToken">A cancellation token for the operation.</param>
    /// <returns>A task that returns a collection of entities as an <see cref="IEnumerable{TEntity}"/>.</returns>
    /// <exception cref="EntityException">Thrown when an error occurs while fetching the range of entities.</exception>
    public Task<IEnumerable<TEntity>> GetRangeAsync(RangeQueryBuilder<TEntity>? builder, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves an entity based on the provided filter expression asynchronously.
    /// </summary>
    /// <param name="filter">The filter expression to apply to the entity.</param>
    /// <param name="cancellationToken">A cancellation token for the operation.</param>
    /// <returns>A task that returns the entity if found, otherwise <c>null</c>.</returns>
    /// <exception cref="EntityException">Thrown when an error occurs while fetching the entity.</exception>
    public Task<TEntity?> GetByFilterAsync(Expression<Func<TEntity, bool>> filter, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves an entity based on the provided builder configuration asynchronously.
    /// </summary>
    /// <param name="builder">The builder that specifies query parameters.</param>
    /// <param name="cancellationToken">A cancellation token for the operation.</param>
    /// <returns>A task that returns the entity if found, otherwise <c>null</c>.</returns>
    /// <exception cref="EntityException">Thrown when an error occurs while fetching the entity.</exception>
    public Task<TEntity?> GetByFilterAsync(SingleQueryBuilder<TEntity> builder, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves an entity by its ID asynchronously.
    /// </summary>
    /// <param name="id">The ID of the entity to retrieve.</param>
    /// <param name="cancellationToken">A cancellation token for the operation.</param>
    /// <returns>A task that returns the entity if found, otherwise <c>null</c>.</returns>
    /// <exception cref="EntityException">Thrown when an error occurs while fetching the entity.</exception>
    public Task<TEntity?> GetByIdAsync(object id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds an entity to the repository asynchronously.
    /// </summary>
    /// <param name="entity">The entity to add.</param>
    /// <param name="cancellationToken">A cancellation token for the operation.</param>
    /// <returns>A task that returns the added entity.</returns>
    /// <exception cref="EntityException">Thrown when an error occurs while adding the entity.</exception>
    public Task<TEntity> AddAsync(TEntity entity, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a range of entities to the repository asynchronously.
    /// </summary>
    /// <param name="entities">The collection of entities to add.</param>
    /// <param name="cancellationToken">A cancellation token for the operation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    /// <exception cref="EntityException">Thrown when an error occurs while adding the entities.</exception>
    public Task AddRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes an entity by its ID asynchronously.
    /// </summary>
    /// <param name="id">The ID of the entity to delete.</param>
    /// <param name="cancellationToken">A cancellation token for the operation.</param>
    /// <returns>A task that returns the deleted entity, or <c>null</c> if not found.</returns>
    /// <exception cref="EntityException">Thrown when an error occurs while deleting the entity.</exception>
    public Task<TEntity?> DeleteByIdAsync(object id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a range of entities by their IDs asynchronously.
    /// </summary>
    /// <param name="identifiers">The collection of IDs of the entities to delete.</param>
    /// <param name="cancellationToken">A cancellation token for the operation.</param>
    /// <returns>A task that returns the collection of deleted entities.</returns>
    /// <exception cref="EntityException">Thrown when an error occurs while deleting the entities.</exception>
    public Task<IEnumerable<TEntity>> DeleteRangeAsync(IEnumerable<object> identifiers, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a range of entities by their objects asynchronously.
    /// </summary>
    /// <param name="entities">The collection of entities to delete.</param>
    /// <param name="cancellationToken">A cancellation token for the operation.</param>
    /// <returns>A task that returns the collection of deleted entities.</returns>
    /// <exception cref="EntityException">Thrown when an error occurs while deleting the entities.</exception>
    public Task<IEnumerable<TEntity>> DeleteRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes an entity based on the provided filter expression asynchronously.
    /// </summary>
    /// <param name="filter">The filter expression to apply to the entity.</param>
    /// <param name="cancellationToken">A cancellation token for the operation.</param>
    /// <returns>A task that returns the deleted entity, or <c>null</c> if not found.</returns>
    /// <exception cref="EntityException">Thrown when an error occurs while deleting the entity.</exception>
    public Task<TEntity?> DeleteByFilterAsync(Expression<Func<TEntity, bool>> filter, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an entity asynchronously.
    /// </summary>
    /// <param name="entity">The entity to update.</param>
    /// <param name="cancellationToken">A cancellation token for the operation.</param>
    /// <returns>A task that returns the updated entity.</returns>
    /// <exception cref="EntityException">Thrown when an error occurs while updating the entity.</exception>
    public Task<TEntity?> UpdateAsync(TEntity entity, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates a range of entities asynchronously.
    /// </summary>
    /// <param name="entities">The collection of entities to update.</param>
    /// <param name="cancellationToken">A cancellation token for the operation.</param>
    /// <returns>A task that returns the updated entities.</returns>
    /// <exception cref="EntityException">Thrown when an error occurs while updating the entities.</exception>
    public Task<IEnumerable<TEntity?>> UpdateRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default);
}

/// <summary>
/// Represents a generic repository pattern with support for a specific <typeparamref name="TDbContext"/> 
/// for performing CRUD operations on entities of type <typeparamref name="TEntity"/>.
/// </summary>
/// <typeparam name="TEntity">The type of the entity. It must inherit from <see cref="EntityBase"/>.</typeparam>
/// <typeparam name="TDbContext">The type of the database context. It must inherit from <see cref="DbContext"/>.</typeparam>
public interface IRepository<TEntity, TDbContext> where TEntity : EntityBase where TDbContext : DbContext
{
    /// <summary>
    /// Gets a queryable collection of entities of type <typeparamref name="TEntity"/>.
    /// </summary>
    /// <returns>An <see cref="IQueryable{TEntity}"/> for querying the entities.</returns>
    /// <exception cref="EntityException">Thrown when an error occurs while fetching the entities.</exception>
    public IQueryable<TEntity> GetQuery();

    /// <summary>
    /// Gets the count of entities that match the provided filter asynchronously.
    /// </summary>
    /// <param name="filter">The filter expression to apply to the entities.</param>
    /// <returns>The total count of entities matching the filter expression as a <see cref="ValueTask{int}"/>.</returns>
    /// <exception cref="EntityException">Thrown when an error occurs while counting the entities.</exception>
    public ValueTask<int> GetCountAsync(Expression<Func<TEntity, bool>>? filter);

    /// <summary>
    /// Retrieves a range of entities based on the provided builder configuration.
    /// </summary>
    /// <param name="builder">The builder that specifies range-related query parameters.</param>
    /// <param name="cancellationToken">A cancellation token for the operation.</param>
    /// <returns>A task that returns a collection of entities as an <see cref="IEnumerable{TEntity}"/>.</returns>
    /// <exception cref="EntityException">Thrown when an error occurs while fetching the range of entities.</exception>
    public Task<IEnumerable<TEntity>> GetRangeAsync(RangeQueryBuilder<TEntity>? builder, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves an entity based on the provided filter expression asynchronously.
    /// </summary>
    /// <param name="filter">The filter expression to apply to the entity.</param>
    /// <param name="cancellationToken">A cancellation token for the operation.</param>
    /// <returns>A task that returns the entity if found, otherwise <c>null</c>.</returns>
    /// <exception cref="EntityException">Thrown when an error occurs while fetching the entity.</exception>
    public Task<TEntity?> GetByFilterAsync(Expression<Func<TEntity, bool>> filter, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves an entity based on the provided builder configuration asynchronously.
    /// </summary>
    /// <param name="builder">The builder that specifies query parameters.</param>
    /// <param name="cancellationToken">A cancellation token for the operation.</param>
    /// <returns>A task that returns the entity if found, otherwise <c>null</c>.</returns>
    /// <exception cref="EntityException">Thrown when an error occurs while fetching the entity.</exception>
    public Task<TEntity?> GetByFilterAsync(SingleQueryBuilder<TEntity> builder, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves an entity by its ID asynchronously.
    /// </summary>
    /// <param name="id">The ID of the entity to retrieve.</param>
    /// <param name="cancellationToken">A cancellation token for the operation.</param>
    /// <returns>A task that returns the entity if found, otherwise <c>null</c>.</returns>
    /// <exception cref="EntityException">Thrown when an error occurs while fetching the entity.</exception>
    public Task<TEntity?> GetByIdAsync(object id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds an entity to the repository asynchronously.
    /// </summary>
    /// <param name="entity">The entity to add.</param>
    /// <param name="cancellationToken">A cancellation token for the operation.</param>
    /// <returns>A task that returns the added entity.</returns>
    /// <exception cref="EntityException">Thrown when an error occurs while adding the entity.</exception>
    public Task<TEntity> AddAsync(TEntity entity, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a range of entities to the repository asynchronously.
    /// </summary>
    /// <param name="entities">The collection of entities to add.</param>
    /// <param name="cancellationToken">A cancellation token for the operation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    /// <exception cref="EntityException">Thrown when an error occurs while adding the entities.</exception>
    public Task AddRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes an entity by its ID asynchronously.
    /// </summary>
    /// <param name="id">The ID of the entity to delete.</param>
    /// <param name="cancellationToken">A cancellation token for the operation.</param>
    /// <returns>A task that returns the deleted entity, or <c>null</c> if not found.</returns>
    /// <exception cref="EntityException">Thrown when an error occurs while deleting the entity.</exception>
    public Task<TEntity?> DeleteByIdAsync(object id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a range of entities by their IDs asynchronously.
    /// </summary>
    /// <param name="identifiers">The collection of IDs of the entities to delete.</param>
    /// <param name="cancellationToken">A cancellation token for the operation.</param>
    /// <returns>A task that returns the collection of deleted entities.</returns>
    /// <exception cref="EntityException">Thrown when an error occurs while deleting the entities.</exception>
    public Task<IEnumerable<TEntity>> DeleteRangeAsync(IEnumerable<object> identifiers, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a range of entities by their objects asynchronously.
    /// </summary>
    /// <param name="entities">The collection of entities to delete.</param>
    /// <param name="cancellationToken">A cancellation token for the operation.</param>
    /// <returns>A task that returns the collection of deleted entities.</returns>
    /// <exception cref="EntityException">Thrown when an error occurs while deleting the entities.</exception>
    public Task<IEnumerable<TEntity>> DeleteRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes an entity based on the provided filter expression asynchronously.
    /// </summary>
    /// <param name="filter">The filter expression to apply to the entity.</param>
    /// <param name="cancellationToken">A cancellation token for the operation.</param>
    /// <returns>A task that returns the deleted entity, or <c>null</c> if not found.</returns>
    /// <exception cref="EntityException">Thrown when an error occurs while deleting the entity.</exception>
    public Task<TEntity?> DeleteByFilterAsync(Expression<Func<TEntity, bool>> filter, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an entity asynchronously.
    /// </summary>
    /// <param name="entity">The entity to update.</param>
    /// <param name="cancellationToken">A cancellation token for the operation.</param>
    /// <returns>A task that returns the updated entity.</returns>
    /// <exception cref="EntityException">Thrown when an error occurs while updating the entity.</exception>
    public Task<TEntity?> UpdateAsync(TEntity entity, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates a range of entities asynchronously.
    /// </summary>
    /// <param name="entities">The collection of entities to update.</param>
    /// <param name="cancellationToken">A cancellation token for the operation.</param>
    /// <returns>A task that returns the updated entities.</returns>
    /// <exception cref="EntityException">Thrown when an error occurs while updating the entities.</exception>
    public Task<IEnumerable<TEntity?>> UpdateRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default);
}