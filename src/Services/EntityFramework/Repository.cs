using Infrastructure.Abstractions.Database;
using Infrastructure.Enums;
using Infrastructure.Exceptions;
using Infrastructure.Services.EntityFramework.Builder;
using Infrastructure.Services.EntityFramework.Context;
using Infrastructure.Services.EntityFramework.Entity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Linq.Expressions;

namespace Infrastructure.Services.EntityFramework;

/// <summary>
/// A generic repository class responsible for performing CRUD operations on entities in a database context.
/// This class implements the <see cref="IRepository{TEntity}"/> and <see cref="IRepository{TEntity, TDbContext}"/> interfaces.
/// It utilizes Entity Framework Core to interact with a database and supports various query and manipulation methods.
/// </summary>
/// <typeparam name="TEntity">The type of the entity being managed by the repository. It must inherit from <see cref="EntityBase"/>.</typeparam>
/// <typeparam name="TDbContext">The type of the database context. It must inherit from <see cref="DbContext"/>.</typeparam>
/// <remarks>
/// This repository class provides methods for querying, adding, updating, and deleting entities in a database.
/// It uses asynchronous operations to ensure non-blocking behavior and supports cancellation through <see cref="CancellationToken"/>.
/// Operations are performed in a thread-safe manner using a <see cref="ReaderWriterLockSlim"/> to avoid race conditions during database access.
/// </remarks>
public sealed class Repository<TEntity, TDbContext> :
    IRepository<TEntity>,
    IRepository<TEntity, TDbContext>,
    IDisposable where TEntity : EntityBase where TDbContext : InfrastructureDbContext
{
    #region fields and constructor

    private readonly Lazy<ILogger<Repository<TEntity, TDbContext>>> _logger;
    private readonly TDbContext _context;
    private readonly Lazy<DbSet<TEntity>> _dbSet;
    private readonly Lazy<string> _tName = new(() => typeof(TEntity).FullName ?? typeof(TEntity).Name);

    private readonly ReaderWriterLockSlim _lock = new(LockRecursionPolicy.NoRecursion);

    private bool disposed;

    private static readonly Lazy<EntityException> _operationCancelledError = new(() => new("The operation was cancelled due to waiting too long for completion or due to manual cancellation"), LazyThreadSafetyMode.ExecutionAndPublication);
    private static readonly Lazy<EntityException> _getRangeError = new(() => new("An error occurred while attempting to retrieve a range of entities"), LazyThreadSafetyMode.ExecutionAndPublication);
    private static readonly Lazy<EntityException> _getCountError = new(() => new("An error occurred while attempting to retrieve count of entities"), LazyThreadSafetyMode.ExecutionAndPublication);
    private static readonly Lazy<EntityException> _getBySingleQueryBuilderError = new(() => new("An error occurred while attempting to retrieve an entity using a single query builder"), LazyThreadSafetyMode.ExecutionAndPublication);
    private static readonly Lazy<EntityException> _getByIdError = new(() => new("An error occurred while attempting to retrieve an entity by its ID"), LazyThreadSafetyMode.ExecutionAndPublication);
    private static readonly Lazy<EntityException> _addError = new(() => new("An error occurred while attempting to add an entity"), LazyThreadSafetyMode.ExecutionAndPublication);
    private static readonly Lazy<EntityException> _addRangeError = new(() => new("An error occurred while attempting to add a range of entities"), LazyThreadSafetyMode.ExecutionAndPublication);
    private static readonly Lazy<EntityException> _deleteByIdError = new(() => new("An error occurred while attempting to delete an entity by its ID"), LazyThreadSafetyMode.ExecutionAndPublication);
    private static readonly Lazy<EntityException> _deleteRangeError = new(() => new("An error occurred while attempting to delete a range of entities by their IDs"), LazyThreadSafetyMode.ExecutionAndPublication);
    private static readonly Lazy<EntityException> _deleteByFilterError = new(() => new("An error occurred while attempting to delete an entity using a filter"), LazyThreadSafetyMode.ExecutionAndPublication);
    private static readonly Lazy<EntityException> _updateError = new(() => new("An error occurred while attempting to update an entity"), LazyThreadSafetyMode.ExecutionAndPublication);
    private static readonly Lazy<EntityException> _updateRangeError = new(() => new("An error occurred while attempting to update a range of entities"), LazyThreadSafetyMode.ExecutionAndPublication);
    private static readonly Lazy<EntityException> _restoreError = new(() => new("An error occurred while attempting to restore a soft-deleted entity"), LazyThreadSafetyMode.ExecutionAndPublication);
    private static readonly Lazy<EntityException> _restoreRangeError = new(() => new("An error occurred while attempting to restore a range of soft-deleted entities"), LazyThreadSafetyMode.ExecutionAndPublication);

    /// <summary>
    /// Initializes a new instance of the <see cref="Repository{TEntity, TDbContext}"/> class.
    /// </summary>
    /// <param name="logger">A logger instance for logging errors and repository activities.</param>
    /// <param name="context">An instance of the database context to interact with the underlying database.</param>
    public Repository(ILogger<Repository<TEntity, TDbContext>> logger, TDbContext context)
    {
        _logger = new Lazy<ILogger<Repository<TEntity, TDbContext>>>(() => logger);
        _context = context ?? throw new InvalidArgumentException("Database context cannot be null");
        _dbSet = new Lazy<DbSet<TEntity>>(() => _context.Set<TEntity>(), LazyThreadSafetyMode.ExecutionAndPublication);
    }

    #endregion

    /// <summary>
    /// Returns an <see cref="IQueryable{TEntity}"/> to perform further queries on the entity set.
    /// <para>Thread-safe method.</para>
    /// </summary>
    /// <returns>An <see cref="IQueryable{TEntity}"/> representing the entity set.</returns>
    public IQueryable<TEntity> GetQuery()
    {
        _lock.EnterReadLock();

        try
        {
            return _dbSet.Value;
        }
        finally
        {
            _lock.ExitReadLock();
        }
    }

    /// <summary>
    /// Asynchronously retrieves the count of entities that match the specified filter.
    /// <para>Thread-safe method.</para>
    /// </summary>
    /// <param name="filter">The filter expression to apply to the entity set.</param>
    /// <returns>The count of entities that match the filter.</returns>
    /// <exception cref="EntityException">Thrown when an error occurs during the count operation.</exception>
    public ValueTask<int> GetCountAsync(Expression<Func<TEntity, bool>>? filter)
    {
        _lock.EnterReadLock();

        try
        {
            IQueryable<TEntity> query = _dbSet.Value;
            if (filter is not null)
                query = query.Where(filter);

            var result = query.Count();
            return new ValueTask<int>(result);
        }
        catch (Exception ex)
        {
            _logger.Value.LogError(ex.ToString(), _tName.Value);
            throw _getCountError.Value;
        }
        finally
        {
            _lock.ExitReadLock();
        }
    }

    /// <summary>
    /// Asynchronously retrieves an entity by its identifier.
    /// <para>Thread-safe method.</para>
    /// </summary>
    /// <param name="id">The identifier of the entity to retrieve.</param>
    /// <param name="cancellationToken">A token to cancel the operation if needed.</param>
    /// <returns>The entity with the specified identifier, or <c>null</c> if not found.</returns>
    /// <exception cref="EntityException">Thrown when an error occurs during the retrieval operation.</exception>
    public async Task<TEntity?> GetByIdAsync(object id, CancellationToken cancellationToken = default)
    {
        _lock.EnterReadLock();

        try
        {
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(Immutable.RepositoryTimeout.GetByIdTimeout));
            cancellationToken = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, cts.Token).Token;

            return await _dbSet.Value.FindAsync([id, cancellationToken], cancellationToken: cancellationToken);
        }
        catch (OperationCanceledException)
        {
            throw _operationCancelledError.Value;
        }
        catch (Exception ex)
        {
            _logger.Value.LogError(ex.ToString(), _tName.Value);
            throw _getByIdError.Value;
        }
        finally
        {
            _lock.ExitReadLock();
        }
    }

    /// <summary>
    /// Asynchronously retrieves the first or last entity that matches the specified filter, including options for ordering and tracking.
    /// <para>Thread-safe method.</para>
    /// </summary>
    /// <param name="builder">A <see cref="SingleQueryBuilder{TEntity}"/> that defines the query criteria.</param>
    /// <param name="cancellationToken">A token to cancel the operation if needed.</param>
    /// <returns>The first or last entity that matches the filter, or <c>null</c> if not found.</returns>
    /// <exception cref="EntityException">Thrown when an error occurs during the retrieval operation.</exception>
    public async Task<TEntity?> GetByFilterAsync(SingleQueryBuilder<TEntity> builder, CancellationToken cancellationToken = default)
    {
        _lock.EnterReadLock();

        try
        {
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(Immutable.RepositoryTimeout.GetByFilterTimeout));
            cancellationToken = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, cts.Token).Token;

            IQueryable<TEntity> query = _dbSet.Value;

            query = builder.Apply(query);
            return builder.TakeFirst ? await query.FirstOrDefaultAsync(cancellationToken) : await query.LastOrDefaultAsync(cancellationToken);
        }
        catch (OperationCanceledException)
        {
            throw _operationCancelledError.Value;
        }
        catch (Exception ex)
        {
            _logger.Value.LogError(ex.ToString(), _tName.Value);
            throw _getBySingleQueryBuilderError.Value;
        }
        finally
        {
            _lock.ExitReadLock();
        }
    }

    /// <summary>
    /// Asynchronously retrieves a range of entities based on the specified query builder.
    /// <para>Thread-safe method.</para>
    /// </summary>
    /// <param name="builder">A <see cref="RangeQueryBuilder{TEntity}"/> that defines the query criteria.</param>
    /// <param name="cancellationToken">A token to cancel the operation if needed.</param>
    /// <returns>A list of entities matching the query criteria.</returns>
    /// <exception cref="EntityException">Thrown when an error occurs during the retrieval operation.</exception>
    public async Task<IEnumerable<TEntity>> GetRangeAsync(RangeQueryBuilder<TEntity>? builder, CancellationToken cancellationToken = default)
    {
        _lock.EnterReadLock();

        try
        {
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(Immutable.RepositoryTimeout.GetRangeTimeout));
            cancellationToken = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, cts.Token).Token;

            IQueryable<TEntity> query = _dbSet.Value;

            if (builder is null)
                return await query.ToListAsync(cancellationToken);

            query = builder.Apply(query);
            return await query.ToListAsync(cancellationToken);
        }
        catch (OperationCanceledException)
        {
            throw _operationCancelledError.Value;
        }
        catch (Exception ex)
        {
            _logger.Value.LogError(ex.ToString(), _tName.Value);
            throw _getRangeError.Value;
        }
        finally
        {
            _lock.ExitReadLock();
        }
    }

    /// <summary>
    /// Asynchronously adds a new entity to the repository.
    /// <para>Thread-safe method.</para>
    /// </summary>
    /// <param name="entity">The entity to add.</param>
    /// <param name="cancellationToken">A token to cancel the operation if needed.</param>
    /// <returns>The added entity.</returns>
    /// <exception cref="EntityException">Thrown when an error occurs during the add operation.</exception>
    public async Task<TEntity> AddAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        _lock.EnterWriteLock();

        try
        {
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(Immutable.RepositoryTimeout.AddTimeout));
            cancellationToken = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, cts.Token).Token;

            await _dbSet.Value.AddAsync(entity, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);

            return entity;
        }
        catch (OperationCanceledException)
        {
            throw _operationCancelledError.Value;
        }
        catch (Exception ex)
        {
            _logger.Value.LogError(ex.ToString(), _tName.Value);
            throw _addError.Value;
        }
        finally
        {
            _lock.ExitWriteLock();
        }
    }

    /// <summary>
    /// Asynchronously adds multiple entities to the repository.
    /// <para>Thread-safe method.</para>
    /// </summary>
    /// <param name="entities">The entities to add.</param>
    /// <param name="cancellationToken">A token to cancel the operation if needed.</param>
    /// <exception cref="EntityException">Thrown when an error occurs during the add operation.</exception>
    public async Task AddRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default)
    {
        _lock.EnterWriteLock();

        try
        {
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(Immutable.RepositoryTimeout.AddRangeTimeout));
            cancellationToken = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, cts.Token).Token;

            await _dbSet.Value.AddRangeAsync(entities, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
        }
        catch (OperationCanceledException)
        {
            throw _operationCancelledError.Value;
        }
        catch (Exception ex)
        {
            _logger.Value.LogError(ex.ToString(), _tName.Value);
            throw _addRangeError.Value;
        }
        finally
        {
            _lock.ExitWriteLock();
        }
    }

    /// <summary>
    /// Asynchronously deletes an entity by its identifier.
    /// <para>Thread-safe method.</para>
    /// </summary>
    /// <param name="id">The identifier of the entity to delete.</param>
    /// <param name="cancellationToken">A token to cancel the operation if needed.</param>
    /// <returns>The deleted entity, or <c>null</c> if no entity was found with the specified identifier.</returns>
    /// <exception cref="EntityException">Thrown when an error occurs during the delete operation.</exception>
    public async Task<TEntity?> DeleteByIdAsync(object id, CancellationToken cancellationToken = default)
    {
        _lock.EnterWriteLock();

        try
        {
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(Immutable.RepositoryTimeout.RemoveByIdTimeout));
            cancellationToken = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, cts.Token).Token;

            var entity = await _dbSet.Value.FindAsync([id, cancellationToken], cancellationToken: cancellationToken);
            if (entity is not null)
            {
                var deletedEntity = _dbSet.Value.Remove(entity).Entity;
                await _context.SaveChangesAsync(cancellationToken);
                return deletedEntity;
            }
            return null;
        }
        catch (OperationCanceledException)
        {
            throw _operationCancelledError.Value;
        }
        catch (Exception ex)
        {
            _logger.Value.LogError(ex.ToString(), _tName.Value);
            throw _deleteByIdError.Value;
        }
        finally
        {
            _lock.ExitWriteLock();
        }
    }

    /// <summary>
    /// Asynchronously deletes an entity based on a specified filter.
    /// <para>Thread-safe method.</para>
    /// </summary>
    /// <param name="filter">An expression that defines the filter to identify the entity to delete.</param>
    /// <param name="cancellationToken">A token to cancel the operation if needed.</param>
    /// <returns>The deleted entity, or <c>null</c> if no entity matches the filter.</returns>
    /// <exception cref="EntityException">Thrown when an error occurs during the delete operation.</exception>
    public async Task<TEntity?> DeleteByFilterAsync(Expression<Func<TEntity, bool>> filter, CancellationToken cancellationToken = default)
    {
        _lock.EnterWriteLock();

        try
        {
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(Immutable.RepositoryTimeout.RemoveByFilterTimeout));
            cancellationToken = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, cts.Token).Token;

            IQueryable<TEntity> query = _dbSet.Value;
            query = query.Where(filter);

            var entity = await query.FirstOrDefaultAsync(cancellationToken);
            if (entity is null)
                return null;

            var deletedEntity = _dbSet.Value.Remove(entity).Entity;
            await _context.SaveChangesAsync(cancellationToken);
            return deletedEntity;
        }
        catch (OperationCanceledException)
        {
            throw _operationCancelledError.Value;
        }
        catch (Exception ex)
        {
            _logger.Value.LogError(ex.ToString(), _tName.Value);
            throw _deleteByFilterError.Value;
        }
        finally
        {
            _lock.ExitWriteLock();
        }
    }

    /// <summary>
    /// Asynchronously deletes a range of entities based on the specified builder parameters.
    /// <para>Thread-safe method.</para>
    /// </summary>
    /// <param name="builder">The builder containing entities or identifiers to delete and removal mode.</param>
    /// <param name="cancellationToken">A token to cancel the operation if needed.</param>
    /// <returns>The collection of deleted entities.</returns>
    /// <exception cref="EntityException">Thrown when an error occurs during the delete operation.</exception>
    /// <remarks>
    /// The deletion mode is determined by the <see cref="RemoveRangeBuilder{TEntity}.RemoveByMode"/> property:
    /// <list type="bullet">
    /// <item><description>If mode is <see cref="RemoveByMode.Entities"/>, deletes the entities directly</description></item>
    /// <item><description>If mode is <see cref="RemoveByMode.Identifiers"/>, first fetches entities by their identifiers before deletion</description></item>
    /// </list>
    /// </remarks>
    public async Task<IEnumerable<TEntity>> DeleteRangeAsync(RemoveRangeBuilder<TEntity> builder, CancellationToken cancellationToken = default)
    {
        _lock.EnterWriteLock();

        try
        {
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(Immutable.RepositoryTimeout.RemoveRangeTimeout));
            cancellationToken = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, cts.Token).Token;

            if (builder.RemoveByMode == RemoveByMode.Entities)
            {
                _context.RemoveRange(builder.Entities);
                await _context.SaveChangesAsync(cancellationToken);
                return builder.Entities;
            }

            if (builder.RemoveByMode == RemoveByMode.Identifiers)
            {
                var entities = new List<TEntity>();

                foreach (var id in builder.Identifiers)
                {
                    var entity = await _dbSet.Value.FindAsync([id, cancellationToken], cancellationToken: cancellationToken);
                    if (entity is not null)
                        entities.Add(entity);
                }

                _context.RemoveRange(entities);
                await _context.SaveChangesAsync(cancellationToken);
                return entities;
            }

            throw _deleteRangeError.Value;
        }
        catch (OperationCanceledException)
        {
            throw _operationCancelledError.Value;
        }
        catch (Exception ex)
        {
            _logger.Value.LogError(ex.ToString(), _tName.Value);
            throw _deleteRangeError.Value;
        }
        finally
        {
            _lock.ExitWriteLock();
        }
    }

    /// <summary>
    /// Asynchronously updates a single entity based on the specified builder parameters.
    /// <para>Thread-safe method that automatically sets update tracking fields.</para>
    /// </summary>
    /// <param name="builder">The builder containing the entity to update and optional user information.</param>
    /// <param name="cancellationToken">A token to cancel the operation if needed.</param>
    /// <returns>The updated entity, or <c>null</c> if the update failed.</returns>
    /// <exception cref="EntityException">Thrown when an error occurs during the update operation.</exception>
    /// <remarks>
    /// This method automatically:
    /// <list type="bullet">
    /// <item><description>Sets the <c>UpdatedBy</c> field to the builder's <see cref="UpdateSingleBuilder{TEntity}.UpdatedByUser"/> value</description></item>
    /// <item><description>Sets the <c>UpdatedAt</c> field to the current UTC timestamp</description></item>
    /// <item><description>Marks all entity properties as modified for the update</description></item>
    /// </list>
    /// </remarks>
    public async Task<TEntity> UpdateAsync(UpdateSingleBuilder<TEntity> builder, CancellationToken cancellationToken = default)
    {
        _lock.EnterWriteLock();

        try
        {
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(Immutable.RepositoryTimeout.UpdateTimeout));
            cancellationToken = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, cts.Token).Token;

            var entity = builder.Entity;

            entity.UpdatedBy = builder.UpdatedByUser;

            _dbSet.Value.Attach(entity);
            _context.Entry(entity).State = EntityState.Modified;

            await _context.SaveChangesAsync(cancellationToken);
            return entity;
        }
        catch (OperationCanceledException)
        {
            throw _operationCancelledError.Value;
        }
        catch (Exception ex)
        {
            _logger.Value.LogError(ex.ToString(), _tName.Value);
            throw _updateError.Value;
        }
        finally
        {
            _lock.ExitWriteLock();
        }
    }

    /// <summary>
    /// Asynchronously updates a collection of entities based on the specified builder parameters.
    /// <para>Thread-safe method that automatically sets update tracking fields for all entities.</para>
    /// </summary>
    /// <param name="builder">The builder containing entities to update and optional user information.</param>
    /// <param name="cancellationToken">A token to cancel the operation if needed.</param>
    /// <returns>A collection of updated entities.</returns>
    /// <exception cref="EntityException">Thrown when an error occurs during the update operation.</exception>
    /// <remarks>
    /// This method performs the following operations for each entity:
    /// <list type="bullet">
    /// <item><description>Sets the <c>UpdatedBy</c> field to the builder's <see cref="UpdateRangeBuilder{TEntity}.UpdatedByUser"/> value</description></item>
    /// <item><description>Sets the <c>UpdatedAt</c> field to the current UTC timestamp</description></item>
    /// <item><description>Marks all entity properties as modified for the update</description></item>
    /// </list>
    /// All updates are performed in a single transaction.
    /// </remarks>
    public async Task<IEnumerable<TEntity?>> UpdateRangeAsync(UpdateRangeBuilder<TEntity> builder, CancellationToken cancellationToken = default)
    {
        _lock.EnterWriteLock();

        try
        {
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(Immutable.RepositoryTimeout.UpdateRangeTimeout));
            cancellationToken = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, cts.Token).Token;

            _dbSet.Value.AttachRange(builder.Entities);

            foreach (var entity in builder.Entities)
            {
                entity.UpdatedBy = builder.UpdatedByUser;
                _context.Entry(entity).State = EntityState.Modified;
            }

            await _context.SaveChangesAsync(cancellationToken);
            return builder.Entities;
        }
        catch (OperationCanceledException)
        {
            throw _operationCancelledError.Value;
        }
        catch (Exception ex)
        {
            _logger.Value.LogError(ex.ToString(), _tName.Value);
            throw _updateRangeError.Value;
        }
        finally
        {
            _lock.ExitWriteLock();
        }
    }

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
    /// This method will:
    /// <list type="bullet">
    ///   <item><description>Mark the entity as not deleted (IsDeleted = false)</description></item>
    ///   <item><description>Update the UpdatedAt timestamp to current UTC time</description></item>
    ///   <item><description>Change entity state to Modified</description></item>
    ///   <item><description>Persist changes to the database</description></item>
    /// </list>
    /// </remarks>
    public async Task<TEntity> RestoreAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        _lock.EnterWriteLock();

        try
        {
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(Immutable.RepositoryTimeout.RestoreTimeout));
            cancellationToken = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, cts.Token).Token;

            _context.Restore(entity);
            await _context.SaveChangesAsync(cancellationToken);

            return entity;
        }
        catch (OperationCanceledException)
        {
            throw _operationCancelledError.Value;
        }
        catch (Exception ex)
        {
            _logger.Value.LogError(ex.ToString(), _tName.Value);
            throw _restoreError.Value;
        }
        finally
        {
            _lock.ExitWriteLock();
        }
    }

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
    /// This method will for each entity:
    /// <list type="bullet">
    ///   <item><description>Mark the entity as not deleted (IsDeleted = false)</description></item>
    ///   <item><description>Update the UpdatedAt timestamp to current UTC time</description></item>
    ///   <item><description>Change entity state to Modified</description></item>
    /// </list>
    /// All changes are persisted in a single database transaction.
    /// </remarks>
    public async Task<IEnumerable<TEntity>> RestoreRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default)
    {
        _lock.EnterWriteLock();

        try
        {
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(Immutable.RepositoryTimeout.RestoreRangeTimeout));
            cancellationToken = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, cts.Token).Token;

            _context.RestoreRange(entities);
            await _context.SaveChangesAsync(cancellationToken);

            return entities;
        }
        catch (OperationCanceledException)
        {
            throw _operationCancelledError.Value;
        }
        catch (Exception ex)
        {
            _logger.Value.LogError(ex.ToString(), _tName.Value);
            throw _restoreRangeError.Value;
        }
        finally
        {
            _lock.ExitWriteLock();
        }
    }

    /// <summary>
    /// Releases the unmanaged resources used by the <see cref="Repository{TEntity, TDbContext}"/> and optionally releases the managed resources.
    /// <para>This method is called by the public <see cref="Dispose()"/> method and the finalizer.</para>
    /// </summary>
    /// <param name="disposing">
    /// <c>true</c> to release both managed and unmanaged resources; 
    /// <c>false</c> to release only unmanaged resources.
    /// </param>
    /// <remarks>
    /// If <paramref name="disposing"/> is <c>true</c>, this method releases all resources held by the repository, 
    /// including the <see cref="SemaphoreSlim"/> used for thread synchronization. 
    /// If <paramref name="disposing"/> is <c>false</c>, only unmanaged resources are released.
    /// This method ensures that resources are not disposed more than once by checking the <see cref="_disposed"/> field.
    /// </remarks>
    private void Dispose(bool disposing)
    {
        if (disposed)
            return;

        if (disposing)
        {
            _lock.Dispose();
        }

        disposed = true;
    }

    /// <summary>
    /// Releases all resources used by the <see cref="Repository{TEntity, TDbContext}"/>.
    /// <para>This method is thread-safe and can be called multiple times without causing exceptions.</para>
    /// </summary>
    /// <remarks>
    /// This method calls the protected <see cref="Dispose(bool)"/> method with <c>true</c> to release both managed and unmanaged resources.
    /// It also suppresses finalization of the current instance by calling <see cref="GC.SuppressFinalize(object)"/>.
    /// After calling this method, the repository should not be used, as its resources have been released.
    /// </remarks>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
}
