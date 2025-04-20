using Infrastructure.Services.EntityFramework.Builder.NoneQuery.Create;
using Infrastructure.Services.EntityFramework.Builder.NoneQuery.Remove;
using Infrastructure.Services.EntityFramework.Builder.NoneQuery.Restore;
using Infrastructure.Services.EntityFramework.Builder.NoneQuery.Update;
using Infrastructure.Services.EntityFramework.Builder.Query;
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
    /// <para>Thread-safe for a single <typeparamref name="TEntity"/> type.</para>
    /// </summary>
    public IQueryable<TEntity> GetQuery();

    /// <summary>
    /// Asynchronously retrieves the count of entities that match the specified filter.
    /// <para>Thread-safe for a single <typeparamref name="TEntity"/> type.</para>
    /// </summary>
    /// <param name="filter">Optional filter expression</param>
    /// <param name="cancellationToken">Cancellation token</param>
    public Task<int> GetCountAsync(Expression<Func<TEntity, bool>>? filter, CancellationToken cancellationToken = default);

    /// <summary>
    /// Asynchronously retrieves an entity by its identifier.
    /// <para>Thread-safe for a single <typeparamref name="TEntity"/> type.</para>
    /// </summary>
    /// <param name="id">Entity identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    public Task<TEntity?> GetByIdAsync(object id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Asynchronously retrieves an entity using a configured query builder.
    /// <para>Thread-safe for a single <typeparamref name="TEntity"/> type.</para>
    /// </summary>
    /// <param name="builder">Configured query builder</param>
    /// <param name="cancellationToken">Cancellation token</param>
    public Task<TEntity?> GetByFilterAsync(SingleQueryBuilder<TEntity> builder, CancellationToken cancellationToken = default);

    /// <summary>
    /// Asynchronously retrieves multiple entities using a configured query builder.
    /// <para>Thread-safe for a single <typeparamref name="TEntity"/> type.</para>
    /// </summary>
    /// <param name="builder">Configured query builder</param>
    /// <param name="cancellationToken">Cancellation token</param>
    public Task<IEnumerable<TEntity>> GetRangeAsync(RangeQueryBuilder<TEntity>? builder, CancellationToken cancellationToken = default);

    /// <summary>
    /// Asynchronously adds an entity using a configured builder.
    /// <para>Thread-safe for a single <typeparamref name="TEntity"/> type.</para>
    /// </summary>
    /// <param name="builder">Configured create builder</param>
    /// <param name="cancellationToken">Cancellation token</param>
    public Task<TEntity> AddAsync(CreateSingleBuilder<TEntity> builder, CancellationToken cancellationToken = default);

    /// <summary>
    /// Asynchronously adds multiple entities using a configured builder.
    /// <para>Thread-safe for a single <typeparamref name="TEntity"/> type.</para>
    /// </summary>
    /// <param name="builder">Configured create builder</param>
    /// <param name="cancellationToken">Cancellation token</param>
    public Task<IEnumerable<TEntity>> AddRangeAsync(CreateRangeBuilder<TEntity> builder, CancellationToken cancellationToken = default);

    /// <summary>
    /// Asynchronously deletes an entity using a configured builder.
    /// <para>Thread-safe for a single <typeparamref name="TEntity"/> type.</para>
    /// </summary>
    /// <param name="builder">Configured delete builder</param>
    /// <param name="cancellationToken">Cancellation token</param>
    public Task<TEntity?> DeleteAsync(RemoveSingleBuilder<TEntity> builder, CancellationToken cancellationToken = default);

    /// <summary>
    /// Asynchronously deletes multiple entities using a configured builder.
    /// <para>Thread-safe for a single <typeparamref name="TEntity"/> type.</para>
    /// </summary>
    /// <param name="builder">Configured delete builder</param>
    /// <param name="cancellationToken">Cancellation token</param>
    public Task<IEnumerable<TEntity>> DeleteRangeAsync(RemoveRangeBuilder<TEntity> builder, CancellationToken cancellationToken = default);

    /// <summary>
    /// Asynchronously updates an entity using a configured builder.
    /// <para>Thread-safe for a single <typeparamref name="TEntity"/> type.</para>
    /// </summary>
    /// <param name="builder">Configured update builder</param>
    /// <param name="cancellationToken">Cancellation token</param>
    public Task<TEntity> UpdateAsync(UpdateSingleBuilder<TEntity> builder, CancellationToken cancellationToken = default);

    /// <summary>
    /// Asynchronously updates multiple entities using a configured builder.
    /// <para>Thread-safe for a single <typeparamref name="TEntity"/> type.</para>
    /// </summary>
    /// <param name="builder">Configured update builder</param>
    /// <param name="cancellationToken">Cancellation token</param>
    public Task<IEnumerable<TEntity>> UpdateRangeAsync(UpdateRangeBuilder<TEntity> builder, CancellationToken cancellationToken = default);

    /// <summary>
    /// Asynchronously restores an entity using a configured builder.
    /// <para>Thread-safe for a single <typeparamref name="TEntity"/> type.</para>
    /// </summary>
    /// <param name="builder">Configured restore builder</param>
    /// <param name="cancellationToken">Cancellation token</param>
    public Task<TEntity> RestoreAsync(RestoreSingleBuilder<TEntity> builder, CancellationToken cancellationToken = default);

    /// <summary>
    /// Asynchronously restores multiple entities using a configured builder.
    /// <para>Thread-safe for a single <typeparamref name="TEntity"/> type.</para>
    /// </summary>
    /// <param name="builder">Configured restore builder</param>
    /// <param name="cancellationToken">Cancellation token</param>
    public Task<IEnumerable<TEntity>> RestoreRangeAsync(RestoreRangeBuilder<TEntity> builder, CancellationToken cancellationToken = default);
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