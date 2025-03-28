using Infrastructure.Abstractions;
using Infrastructure.Enums;
using Infrastructure.Exceptions;
using Infrastructure.Services.EntityFramework.Builder;
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
public class Repository<TEntity, TDbContext> :
    IRepository<TEntity>,
    IRepository<TEntity, TDbContext>,
    IDisposable where TEntity : EntityBase where TDbContext : DbContext
{
    #region fields and constructor

    private readonly Lazy<ILogger<Repository<TEntity, TDbContext>>> _logger;
    private readonly TDbContext _context;
    private readonly Lazy<DbSet<TEntity>> _dbSet;
    private readonly Lazy<string> _tName = new(() => typeof(TEntity).FullName ?? typeof(TEntity).Name);

    private readonly ReaderWriterLockSlim _lock = new(LockRecursionPolicy.NoRecursion);

    private bool _disposed;

    private static readonly Lazy<EntityException> _operationCancelledError = new(() => new("The operation was cancelled due to waiting too long for completion or due to manual cancellation"), LazyThreadSafetyMode.ExecutionAndPublication);
    private static readonly Lazy<EntityException> _getRangeError = new(() => new("An error occurred while attempting to retrieve a range of entities"), LazyThreadSafetyMode.ExecutionAndPublication);
    private static readonly Lazy<EntityException> _getCountError = new(() => new("An error occurred while attempting to retrieve count of entities"), LazyThreadSafetyMode.ExecutionAndPublication);
    private static readonly Lazy<EntityException> _getByFilterError = new(() => new("An error occurred while attempting to retrieve an entity using a filter"), LazyThreadSafetyMode.ExecutionAndPublication);
    private static readonly Lazy<EntityException> _getBySingleQueryBuilderError = new(() => new("An error occurred while attempting to retrieve an entity using a single query builder"), LazyThreadSafetyMode.ExecutionAndPublication);
    private static readonly Lazy<EntityException> _getByIdError = new(() => new("An error occurred while attempting to retrieve an entity by its ID"), LazyThreadSafetyMode.ExecutionAndPublication);
    private static readonly Lazy<EntityException> _addError = new(() => new("An error occurred while attempting to add an entity"), LazyThreadSafetyMode.ExecutionAndPublication);
    private static readonly Lazy<EntityException> _addRangeError = new(() => new("An error occurred while attempting to add a range of entities"), LazyThreadSafetyMode.ExecutionAndPublication);
    private static readonly Lazy<EntityException> _deleteByIdError = new(() => new("An error occurred while attempting to delete an entity by its ID"), LazyThreadSafetyMode.ExecutionAndPublication);
    private static readonly Lazy<EntityException> _deleteRangeByIdsError = new(() => new("An error occurred while attempting to delete a range of entities by their IDs"), LazyThreadSafetyMode.ExecutionAndPublication);
    private static readonly Lazy<EntityException> _deleteRangeByEntitiesError = new(() => new("An error occurred while attempting to delete a range of entities"), LazyThreadSafetyMode.ExecutionAndPublication);
    private static readonly Lazy<EntityException> _deleteByFilterError = new(() => new("An error occurred while attempting to delete an entity using a filter"), LazyThreadSafetyMode.ExecutionAndPublication);
    private static readonly Lazy<EntityException> _updateError = new(() => new("An error occurred while attempting to update an entity"), LazyThreadSafetyMode.ExecutionAndPublication);
    private static readonly Lazy<EntityException> _updateRangeError = new(() => new("An error occurred while attempting to update a range of entities"), LazyThreadSafetyMode.ExecutionAndPublication);

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

    /// <summary>
    /// Finalizes an instance of the <see cref="Repository{TEntity, TDbContext}"/> class.
    /// <para>This destructor is called by the garbage collector when the object is being finalized.</para>
    /// </summary>
    /// <remarks>
    /// The finalizer ensures that unmanaged resources are released if the <see cref="Dispose()"/> method 
    /// was not called explicitly. It calls the <see cref="Dispose(bool)"/> method with <c>false</c> to release 
    /// only unmanaged resources. Managed resources are not disposed in this case, as they are assumed to have 
    /// already been released by the <see cref="Dispose()"/> method.
    /// </remarks>
    ~Repository()
    {
        Dispose(false);
    }

    #endregion

    /// <summary>
    /// Returns an <see cref="IQueryable{TEntity}"/> to perform further queries on the entity set.
    /// <para>Thread-safe method.</para>
    /// </summary>
    /// <returns>An <see cref="IQueryable{TEntity}"/> representing the entity set.</returns>
    public virtual IQueryable<TEntity> GetQuery()
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
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(Immutable.GetByIdAwait));
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
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(Immutable.GetByFilterAwait));
            cancellationToken = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, cts.Token).Token;

            IQueryable<TEntity> query = _dbSet.Value;

            if (builder.IncludeQuery is not null)
                query = builder.IncludeQuery;

            if (builder.AsNoTracking)
                query = query.AsNoTracking();

            if (builder.Filter is not null)
                query = query.Where(builder.Filter);

            if (builder.OrderExpression is not null)
                query = builder.OrderByDesc ? query.OrderByDescending(builder.OrderExpression) : query.OrderBy(builder.OrderExpression);

            query = query.AsSplitQuery();
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
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(Immutable.GetRangeAwait));
            cancellationToken = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, cts.Token).Token;

            IQueryable<TEntity> query = _dbSet.Value;

            if (builder is null)
                return await query.ToListAsync(cancellationToken);

            if (builder.IncludeQuery is not null)
                query = builder.IncludeQuery;

            if (builder.AsNoTracking)
                query = query.AsNoTracking();

            if (builder.Filter is not null)
                query = query.Where(builder.Filter);

            if (builder.OrderExpression is not null)
                query = builder.OrderByDesc ? query.OrderByDescending(builder.OrderExpression) : query.OrderBy(builder.OrderExpression);

            query = query.Skip(builder.Skip).Take(builder.Take);

            query = query.AsSplitQuery();
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
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(Immutable.AddAwait));
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
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(Immutable.AddRangeAwait));
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
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(Immutable.RemoveByIdAwait));
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
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(Immutable.RemoveByFilterAwait));
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
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(Immutable.RemoveRangeAwait));
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

            throw _deleteRangeByIdsError.Value;
        }
        catch (OperationCanceledException)
        {
            throw _operationCancelledError.Value;
        }
        catch (Exception ex)
        {
            _logger.Value.LogError(ex.ToString(), _tName.Value);
            throw _deleteRangeByIdsError.Value;
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
    public async Task<TEntity?> UpdateAsync(UpdateSingleBuilder<TEntity> builder, CancellationToken cancellationToken = default)
    {
        _lock.EnterWriteLock();

        try
        {
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(Immutable.UpdateAwait));
            cancellationToken = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, cts.Token).Token;

            var entity = builder.Entity;

            entity.UpdatedBy = builder.UpdatedByUser;
            entity.UpdatedAt = DateTimeOffset.UtcNow;

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
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(Immutable.UpdateRangeAwait));
            cancellationToken = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, cts.Token).Token;

            foreach (var entity in builder.Entities)
            {
                entity.UpdatedBy = builder.UpdatedByUser;
                entity.UpdatedAt = DateTimeOffset.UtcNow;

                _dbSet.Value.Attach(entity);
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
    protected virtual void Dispose(bool disposing)
    {
        if (_disposed)
            return;

        if (disposing)
        {
            _lock.Dispose();
        }

        _disposed = true;
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
