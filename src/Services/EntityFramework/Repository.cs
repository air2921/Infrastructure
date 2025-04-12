using Infrastructure.Abstractions.Database;
using Infrastructure.Enums;
using Infrastructure.Exceptions;
using Infrastructure.Services.EntityFramework.Builder.NoneQuery.Create;
using Infrastructure.Services.EntityFramework.Builder.NoneQuery.Remove;
using Infrastructure.Services.EntityFramework.Builder.NoneQuery.Restore;
using Infrastructure.Services.EntityFramework.Builder.NoneQuery.Update;
using Infrastructure.Services.EntityFramework.Builder.Query;
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
    IDisposable where TEntity : EntityBase where TDbContext : EntityFrameworkContext
{
    #region fields and constructor

    private readonly Lazy<ILogger<Repository<TEntity, TDbContext>>> _logger;
    private readonly TDbContext _context;
    private readonly Lazy<DbSet<TEntity>> _dbSet;
    private readonly Lazy<string> _tName = new(() => typeof(TEntity).FullName ?? typeof(TEntity).Name);

    private readonly ReaderWriterLockSlim _lock = new(LockRecursionPolicy.NoRecursion);

    private bool disposed;

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
    /// <exception cref="ObjectDisposedException">Thrown if the repository has been disposed.</exception>
    public IQueryable<TEntity> GetQuery()
    {
        ObjectDisposedException.ThrowIf(disposed, this);
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
    /// <exception cref="ObjectDisposedException">Thrown if the repository has been disposed.</exception>
    public ValueTask<int> GetCountAsync(Expression<Func<TEntity, bool>>? filter)
    {
        ObjectDisposedException.ThrowIf(disposed, this);
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
            _logger.Value.LogError(ex, "An error occurred while attempting to retrieve count of entities", [_tName.Value]);
            throw new EntityException("An error occurred while attempting to retrieve count of entities");
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
    /// <exception cref="ObjectDisposedException">Thrown if the repository has been disposed.</exception>
    public async Task<TEntity?> GetByIdAsync(object id, CancellationToken cancellationToken = default)
    {
        ObjectDisposedException.ThrowIf(disposed, this);
        _lock.EnterReadLock();

        try
        {
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(Immutable.RepositoryTimeout.GetByIdTimeout));
            cancellationToken = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, cts.Token).Token;

            return await _dbSet.Value.FindAsync([id, cancellationToken], cancellationToken: cancellationToken);
        }
        catch (OperationCanceledException)
        {
            throw new EntityException("The operation was cancelled due to waiting too long for completion or due to manual cancellation");
        }
        catch (Exception ex)
        {
            _logger.Value.LogError(ex, "An error occurred while attempting to retrieve an entity by its ID", [_tName.Value, id]);
            throw new EntityException("An error occurred while attempting to retrieve an entity by its ID");
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
    /// <exception cref="ObjectDisposedException">Thrown if the repository has been disposed.</exception>
    public async Task<TEntity?> GetByFilterAsync(SingleQueryBuilder<TEntity> builder, CancellationToken cancellationToken = default)
    {
        ObjectDisposedException.ThrowIf(disposed, this);
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
            throw new EntityException("The operation was cancelled due to waiting too long for completion or due to manual cancellation");
        }
        catch (Exception ex)
        {
            _logger.Value.LogError(ex, "An error occurred while attempting to retrieve an entity using a single query builder", [_tName.Value]);
            throw new EntityException("An error occurred while attempting to retrieve an entity using a single query builder");
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
    /// <exception cref="ObjectDisposedException">Thrown if the repository has been disposed.</exception>
    public async Task<IEnumerable<TEntity>> GetRangeAsync(RangeQueryBuilder<TEntity>? builder, CancellationToken cancellationToken = default)
    {
        ObjectDisposedException.ThrowIf(disposed, this);
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
            throw new EntityException("The operation was cancelled due to waiting too long for completion or due to manual cancellation");
        }
        catch (Exception ex)
        {
            _logger.Value.LogError(ex, "An error occurred while attempting to retrieve a range of entities", [_tName.Value]);
            throw new EntityException("An error occurred while attempting to retrieve a range of entities");
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
    /// <param name="builder">Preconfigured builder containing the entity to add.</param>
    /// <param name="cancellationToken">A token to cancel the operation if needed.</param>
    /// <returns>The added entity.</returns>
    /// <exception cref="EntityException">Thrown when an error occurs during the add operation.</exception>
    /// <exception cref="ObjectDisposedException">Thrown if the repository has been disposed.</exception>
    public async Task<TEntity> AddAsync(CreateSingleBuilder<TEntity> builder, CancellationToken cancellationToken = default)
    {
        ObjectDisposedException.ThrowIf(disposed, this);
        _lock.EnterWriteLock();

        try
        {
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(Immutable.RepositoryTimeout.AddTimeout));
            cancellationToken = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, cts.Token).Token;

            builder.Entity.CreatedBy = builder.CreatedByUser;
            await _dbSet.Value.AddAsync(builder.Entity, cancellationToken);
            
            if (builder.SaveChanges)
                await _context.SaveChangesAsync(cancellationToken);

            return builder.Entity;
        }
        catch (OperationCanceledException)
        {
            throw new EntityException("The operation was cancelled due to waiting too long for completion or due to manual cancellation");
        }
        catch (Exception ex)
        {
            _logger.Value.LogError(ex, "An error occurred while attempting to add an entity", [_tName.Value, new { builder.Entity.Id }]);
            throw new EntityException("An error occurred while attempting to add an entity");
        }
        finally
        {
            _lock.ExitWriteLock();
        }
    }

    /// <summary>
    /// Asynchronously adds multiple entities to the repository using a configured builder.
    /// <para>Thread-safe method.</para>
    /// </summary>
    /// <param name="builder">Preconfigured builder containing the entities to add.</param>
    /// <param name="cancellationToken">A token to cancel the operation if needed.</param>
    /// <returns>The collection of added entities.</returns>
    /// <exception cref="EntityException">
    /// Thrown when an error occurs during the add operation or when operation is canceled.
    /// </exception>
    /// <exception cref="ObjectDisposedException">Thrown if the repository has been disposed.</exception>
    public async Task<IEnumerable<TEntity>> AddRangeAsync(CreateRangeBuilder<TEntity> builder, CancellationToken cancellationToken = default)
    {
        ObjectDisposedException.ThrowIf(disposed, this);
        _lock.EnterWriteLock();

        try
        {
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(Immutable.RepositoryTimeout.AddRangeTimeout));
            cancellationToken = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, cts.Token).Token;

            foreach (var entity in builder.Entities)
                entity.CreatedBy = builder.CreatedByUser;

            await _dbSet.Value.AddRangeAsync(builder.Entities, cancellationToken);
            
            if (builder.SaveChanges)
                await _context.SaveChangesAsync(cancellationToken);

            return builder.Entities;
        }
        catch (OperationCanceledException)
        {
            throw new EntityException("The operation was cancelled due to waiting too long for completion or due to manual cancellation");
        }
        catch (Exception ex)
        {
            _logger.Value.LogError(ex, "An error occurred while attempting to add a range of entities", [_tName.Value, builder.Entities.Select(x => new { x.Id })]);
            throw new EntityException("An error occurred while attempting to add a range of entities");
        }
        finally
        {
            _lock.ExitWriteLock();
        }
    }

    /// <summary>
    /// Asynchronously deletes an entity using a configured builder.
    /// <para>Thread-safe method.</para>
    /// </summary>
    /// <param name="builder">Builder with entity deletion parameters</param>
    /// <param name="cancellationToken">A token to cancel the operation if needed</param>
    /// <returns>The deleted entity, or null if not found</returns>
    /// <exception cref="EntityException">Thrown when an error occurs during deletion</exception>
    /// <exception cref="ObjectDisposedException">Thrown if the repository has been disposed.</exception>
    public async Task<TEntity?> DeleteAsync(RemoveSingleBuilder<TEntity> builder, CancellationToken cancellationToken = default)
    {
        ObjectDisposedException.ThrowIf(disposed, this);
        _lock.EnterWriteLock();

        try
        {
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(Immutable.RepositoryTimeout.RemoveByFilterTimeout));
            cancellationToken = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, cts.Token).Token;

            TEntity? entity = default;

            if (builder.RemoveByMode == RemoveByMode.Entity)
                entity = builder.Entity;

            if (builder.RemoveByMode == RemoveByMode.Identifier)
                entity = await _dbSet.Value.FindAsync([builder.Id, cancellationToken], cancellationToken: cancellationToken);

            if (builder.RemoveByMode == RemoveByMode.Filter && builder.Filter is not null)
                entity = await _dbSet.Value.FirstOrDefaultAsync(builder.Filter, cancellationToken);

            if (entity is null)
                return null;

            var deletedEntity = _dbSet.Value.Remove(entity).Entity;
            
            if (builder.SaveChanges)
                await _context.SaveChangesAsync(cancellationToken);

            return deletedEntity;
        }
        catch (OperationCanceledException)
        {
            throw new EntityException("The operation was cancelled due to waiting too long for completion or due to manual cancellation");
        }
        catch (Exception ex)
        {
            _logger.Value.LogError(ex, "An error occurred while attempting to delete an entity using a filter", [_tName.Value]);
            throw new EntityException("An error occurred while attempting to delete an entity using a filter");
        }
        finally
        {
            _lock.ExitWriteLock();
        }
    }

    /// <summary>
    /// Asynchronously deletes multiple entities using a configured builder.
    /// <para>Thread-safe method.</para>
    /// </summary>
    /// <param name="builder">Configured builder with deletion parameters</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Deleted entities</returns>
    /// <exception cref="EntityException">On deletion error</exception>
    /// <exception cref="ObjectDisposedException">Thrown if the repository has been disposed.</exception>
    public async Task<IEnumerable<TEntity>> DeleteRangeAsync(RemoveRangeBuilder<TEntity> builder, CancellationToken cancellationToken = default)
    {
        ObjectDisposedException.ThrowIf(disposed, this);
        _lock.EnterWriteLock();

        try
        {
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(Immutable.RepositoryTimeout.RemoveRangeTimeout));
            cancellationToken = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, cts.Token).Token;

            if (builder.RemoveByMode == RemoveByMode.Entity)
            {
                _context.RemoveRange(builder.Entities);

                if (builder.SaveChanges)
                    await _context.SaveChangesAsync(cancellationToken);

                return builder.Entities;
            }

            if (builder.RemoveByMode == RemoveByMode.Identifier)
            {
                var entities = await _dbSet.Value
                    .Where(e => builder.Identifiers.Contains(e.Id))
                    .ToListAsync(cancellationToken);

                _context.RemoveRange(entities);

                if (builder.SaveChanges)
                    await _context.SaveChangesAsync(cancellationToken);

                return entities;
            }

            if (builder.RemoveByMode == RemoveByMode.Filter && builder.Filter is not null)
            {
                var entities = await _dbSet.Value
                    .Where(builder.Filter)
                    .ToListAsync(cancellationToken);

                _context.RemoveRange(entities);

                if (builder.SaveChanges)
                    await _context.SaveChangesAsync(cancellationToken);

                return entities;
            }

            throw new EntityException($"Invalid {nameof(builder.RemoveByMode)}");
        }
        catch (OperationCanceledException)
        {
            throw new EntityException("The operation was cancelled due to waiting too long for completion or due to manual cancellation");
        }
        catch (Exception ex)
        {
            _logger.Value.LogError(ex, "An error occurred while attempting to delete a range of entities by their IDs", [_tName.Value]);
            throw new EntityException("An error occurred while attempting to delete a range of entities by their IDs");
        }
        finally
        {
            _lock.ExitWriteLock();
        }
    }

    /// <summary>
    /// Asynchronously updates an entity using a configured builder.
    /// <para>Thread-safe method.</para>
    /// </summary>
    /// <param name="builder">Builder with entity update parameters</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The updated entity</returns>
    /// <exception cref="EntityException">On update error</exception>
    /// <exception cref="ObjectDisposedException">Thrown if the repository has been disposed.</exception>
    public async Task<TEntity> UpdateAsync(UpdateSingleBuilder<TEntity> builder, CancellationToken cancellationToken = default)
    {
        ObjectDisposedException.ThrowIf(disposed, this);
        _lock.EnterWriteLock();

        try
        {
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(Immutable.RepositoryTimeout.UpdateTimeout));
            cancellationToken = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, cts.Token).Token;

            var entity = builder.Entity;
            entity.UpdatedBy = builder.UpdatedByUser;

            _dbSet.Value.Attach(entity);
            _context.Entry(entity).State = EntityState.Modified;

            if (builder.SaveChanges)
                await _context.SaveChangesAsync(cancellationToken);

            return entity;
        }
        catch (OperationCanceledException)
        {
            throw new EntityException("The operation was cancelled due to waiting too long for completion or due to manual cancellation");
        }
        catch (Exception ex)
        {
            _logger.Value.LogError(ex, "An error occurred while attempting to update an entity", [_tName.Value, builder.Entity.Id]);
            throw new EntityException("An error occurred while attempting to update an entity");
        }
        finally
        {
            _lock.ExitWriteLock();
        }
    }

    /// <summary>
    /// Asynchronously updates multiple entities using a configured builder.
    /// <para>Thread-safe batch update operation.</para>
    /// </summary>
    /// <param name="builder">Configured builder with update parameters</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Updated entities</returns>
    /// <exception cref="EntityException">On update error</exception>
    /// <exception cref="ObjectDisposedException">Thrown if the repository has been disposed.</exception>
    public async Task<IEnumerable<TEntity>> UpdateRangeAsync(UpdateRangeBuilder<TEntity> builder, CancellationToken cancellationToken = default)
    {
        ObjectDisposedException.ThrowIf(disposed, this);
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

            if (builder.SaveChanges)
                await _context.SaveChangesAsync(cancellationToken);

            return builder.Entities;
        }
        catch (OperationCanceledException)
        {
            throw new EntityException("The operation was cancelled due to waiting too long for completion or due to manual cancellation");
        }
        catch (Exception ex)
        {
            _logger.Value.LogError(ex, "An error occurred while attempting to update a range of entities", [_tName.Value, builder.Entities.Select(x => new { x.Id })]);
            throw new EntityException("An error occurred while attempting to update a range of entities");
        }
        finally
        {
            _lock.ExitWriteLock();
        }
    }

    /// <summary>
    /// Asynchronously restores a soft-deleted entity using a configured builder.
    /// <para>Thread-safe method.</para>
    /// </summary>
    /// <param name="builder">Builder with entity restore parameters</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The restored entity</returns>
    /// <exception cref="EntityException">On restore error</exception>
    /// <exception cref="ObjectDisposedException">Thrown if the repository has been disposed.</exception>
    public async Task<TEntity> RestoreAsync(RestoreSingleBuilder<TEntity> builder, CancellationToken cancellationToken = default)
    {
        ObjectDisposedException.ThrowIf(disposed, this);
        _lock.EnterWriteLock();

        try
        {
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(Immutable.RepositoryTimeout.RestoreTimeout));
            cancellationToken = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, cts.Token).Token;

            _context.Restore(builder.Entity);

            if (builder.SaveChanges)
                await _context.SaveChangesAsync(cancellationToken);

            return builder.Entity;
        }
        catch (OperationCanceledException)
        {
            throw new EntityException("The operation was cancelled due to waiting too long for completion or due to manual cancellation");
        }
        catch (Exception ex)
        {
            _logger.Value.LogError(ex, "An error occurred while attempting to restore a soft-deleted entity", [_tName.Value, builder.Entity.Id]);
            throw new EntityException("An error occurred while attempting to restore a soft-deleted entity");
        }
        finally
        {
            _lock.ExitWriteLock();
        }
    }

    /// <summary>
    /// Asynchronously restores multiple soft-deleted entities using a configured builder.
    /// <para>Thread-safe batch operation.</para>
    /// </summary>
    /// <param name="builder">Builder with entities to restore</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Restored entities</returns>
    /// <exception cref="EntityException">On restore error</exception>
    /// <exception cref="ObjectDisposedException">Thrown if the repository has been disposed.</exception>
    public async Task<IEnumerable<TEntity>> RestoreRangeAsync(RestoreRangeBuilder<TEntity> builder, CancellationToken cancellationToken = default)
    {
        ObjectDisposedException.ThrowIf(disposed, this);
        _lock.EnterWriteLock();

        try
        {
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(Immutable.RepositoryTimeout.RestoreRangeTimeout));
            cancellationToken = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, cts.Token).Token;

            _context.RestoreRange(builder.Entities);

            if (builder.SaveChanges)
                await _context.SaveChangesAsync(cancellationToken);

            return builder.Entities;
        }
        catch (OperationCanceledException)
        {
            throw new EntityException("The operation was cancelled due to waiting too long for completion or due to manual cancellation");
        }
        catch (Exception ex)
        {
            _logger.Value.LogError(ex, "An error occurred while attempting to restore a range of soft-deleted entities", [_tName.Value, builder.Entities.Select(x => new { x.Id })]);
            throw new EntityException("An error occurred while attempting to restore a range of soft-deleted entities");
        }
        finally
        {
            _lock.ExitWriteLock();
        }
    }

    /// <summary>
    /// Asynchronously saves all changes made in this repository to the underlying database.
    /// <para>Thread-safe method that persists all pending changes (inserts, updates, deletes).</para>
    /// </summary>
    /// <param name="cancellationToken">A token to cancel the operation if needed.</param>
    /// <returns>A task representing the asynchronous save operation.</returns>
    /// <exception cref="EntityException">Thrown when an error occurs during the save operation.</exception>
    /// <remarks>
    /// This method will persist all changes tracked by the current DbContext instance.
    /// It's typically used after performing multiple operations when you want to
    /// ensure all changes are committed together.
    /// </remarks>
    /// <exception cref="ObjectDisposedException">Thrown if the repository has been disposed.</exception>
    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        ObjectDisposedException.ThrowIf(disposed, this);

        try
        {
            await _context.SaveChangesAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.Value.LogError(ex, "An error occurred while attempting to save changes", [_tName.Value]);
            throw new EntityException("An error occurred while attempting to save changes");
        }
    }

    /// <summary>
    /// Synchronously saves all changes made in this repository to the underlying database.
    /// <para>Thread-safe method that persists all pending changes (inserts, updates, deletes).</para>
    /// </summary>
    /// <exception cref="EntityException">Thrown when an error occurs during the save operation.</exception>
    /// <remarks>
    /// This method will persist all changes tracked by the current DbContext instance.
    /// Consider using <see cref="SaveChangesAsync"/> for non-blocking operations.
    /// </remarks>
    /// <exception cref="ObjectDisposedException">Thrown if the repository has been disposed.</exception>
    public void SaveChanges()
    {
        ObjectDisposedException.ThrowIf(disposed, this);

        try
        {

            _context.SaveChanges();
        }
        catch (Exception ex)
        {
            _logger.Value.LogError(ex, "An error occurred while attempting to save changes", [_tName.Value]);
            throw new EntityException("An error occurred while attempting to save changes");
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
