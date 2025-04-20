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
using static Infrastructure.InfrastructureImmutable.RepositoryTimeout;

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
/// </remarks>
public sealed class Repository<TEntity, TDbContext> :
    IRepository<TEntity>,
    IRepository<TEntity, TDbContext> where TEntity : EntityBase where TDbContext : EntityFrameworkContext
{
    #region fields and constructor

    private readonly ILogger<Repository<TEntity, TDbContext>> _logger;
    private readonly TDbContext _context;
    private readonly DbSet<TEntity> _dbSet;
    private readonly Lazy<string> _tName = new(() => typeof(TEntity).FullName ?? typeof(TEntity).Name);

    /// <summary>
    /// Initializes a new instance of the <see cref="Repository{TEntity, TDbContext}"/> class.
    /// </summary>
    /// <param name="logger">A logger instance for logging errors and repository activities.</param>
    /// <param name="context">An instance of the database context to interact with the underlying database.</param>
    public Repository(ILogger<Repository<TEntity, TDbContext>> logger, TDbContext context)
    {
        _logger = logger;
        _context = context ?? throw new InvalidArgumentException("Database context cannot be null");
        _dbSet = _context.Set<TEntity>();
    }

    #endregion

    /// <summary>
    /// Returns an <see cref="IQueryable{TEntity}"/> to perform further queries on the entity set.
    /// </summary>
    /// <returns>An <see cref="IQueryable{TEntity}"/> representing the entity set.</returns>
    public IQueryable<TEntity> GetQuery()
        => _dbSet;

    /// <summary>
    /// Asynchronously retrieves the count of entities that match the specified filter.
    /// </summary>
    /// <param name="filter">The filter expression to apply to the entity set.</param>
    /// <param name="cancellationToken">A token to cancel the operation if needed.</param>
    /// <returns>The count of entities that match the filter.</returns>
    /// <exception cref="EntityException">Thrown when an error occurs during the count operation.</exception>
    public async Task<int> GetCountAsync(Expression<Func<TEntity, bool>>? filter, CancellationToken cancellationToken = default)
    {
        try
        {
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(GetCountTimeout));
            cancellationToken = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, cts.Token).Token;

            IQueryable<TEntity> query = _dbSet;
            if (filter is not null)
                query = query.Where(filter);

            var result = await query.CountAsync(cancellationToken);
            return result;
        }
        catch (OperationCanceledException)
        {
            throw new EntityException("The operation was cancelled due to waiting too long for completion or due to manual cancellation");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while attempting to retrieve count of entities. {entity}", _tName.Value);
            throw new EntityException("An error occurred while attempting to retrieve count of entities");
        }
    }

    /// <summary>
    /// Asynchronously retrieves an entity by its identifier.
    /// </summary>
    /// <param name="id">The identifier of the entity to retrieve.</param>
    /// <param name="cancellationToken">A token to cancel the operation if needed.</param>
    /// <returns>The entity with the specified identifier, or <c>null</c> if not found.</returns>
    /// <exception cref="EntityException">Thrown when an error occurs during the retrieval operation.</exception>
    public async Task<TEntity?> GetByIdAsync(object id, CancellationToken cancellationToken = default)
    {
        try
        {
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(GetByIdTimeout));
            cancellationToken = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, cts.Token).Token;

            return await _dbSet.FindAsync([id, cancellationToken], cancellationToken: cancellationToken);
        }
        catch (OperationCanceledException)
        {
            throw new EntityException("The operation was cancelled due to waiting too long for completion or due to manual cancellation");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while attempting to retrieve an entity by its ID. {entity}, {id}", _tName.Value, id);
            throw new EntityException("An error occurred while attempting to retrieve an entity by its ID");
        }
    }

    /// <summary>
    /// Asynchronously retrieves the first or last entity that matches the specified filter, including options for ordering and tracking.
    /// </summary>
    /// <param name="builder">A <see cref="SingleQueryBuilder{TEntity}"/> that defines the query criteria.</param>
    /// <param name="cancellationToken">A token to cancel the operation if needed.</param>
    /// <returns>The first or last entity that matches the filter, or <c>null</c> if not found.</returns>
    /// <exception cref="EntityException">Thrown when an error occurs during the retrieval operation.</exception>
    public async Task<TEntity?> GetByFilterAsync(SingleQueryBuilder<TEntity> builder, CancellationToken cancellationToken = default)
    {
        try
        {
            var timeout = builder.Timeout == TimeSpan.Zero ? TimeSpan.FromSeconds(GetByFilterTimeout) : builder.Timeout;
            using var cts = new CancellationTokenSource(timeout);
            cancellationToken = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, cts.Token).Token;

            IQueryable<TEntity> query = _dbSet;

            query = builder.Apply(query);
            return builder.TakeFirst ? await query.FirstOrDefaultAsync(cancellationToken) : await query.LastOrDefaultAsync(cancellationToken);
        }
        catch (OperationCanceledException)
        {
            throw new EntityException("The operation was cancelled due to waiting too long for completion or due to manual cancellation");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while attempting to retrieve an entity using a single query builder. {entity}", _tName.Value);
            throw new EntityException("An error occurred while attempting to retrieve an entity using a single query builder");
        }
    }

    /// <summary>
    /// Asynchronously retrieves a range of entities based on the specified query builder.
    /// </summary>
    /// <param name="builder">A <see cref="RangeQueryBuilder{TEntity}"/> that defines the query criteria.</param>
    /// <param name="cancellationToken">A token to cancel the operation if needed.</param>
    /// <returns>A list of entities matching the query criteria.</returns>
    /// <exception cref="EntityException">Thrown when an error occurs during the retrieval operation.</exception>
    public async Task<IEnumerable<TEntity>> GetRangeAsync(RangeQueryBuilder<TEntity>? builder, CancellationToken cancellationToken = default)
    {
        try
        {
            var timeout = builder?.Timeout == TimeSpan.Zero || builder is null ? TimeSpan.FromSeconds(GetRangeTimeout) : builder.Timeout;
            using var cts = new CancellationTokenSource(timeout);
            cancellationToken = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, cts.Token).Token;

            IQueryable<TEntity> query = _dbSet;

            if (builder is null)
                return await query.ToArrayAsync(cancellationToken);

            query = builder.Apply(query);
            return await query.ToArrayAsync(cancellationToken);
        }
        catch (OperationCanceledException)
        {
            throw new EntityException("The operation was cancelled due to waiting too long for completion or due to manual cancellation");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while attempting to retrieve a range of entities. {entity}", _tName.Value);
            throw new EntityException("An error occurred while attempting to retrieve a range of entities");
        }
    }

    /// <summary>
    /// Asynchronously adds a new entity to the repository.
    /// </summary>
    /// <param name="builder">Preconfigured builder containing the entity to add.</param>
    /// <param name="cancellationToken">A token to cancel the operation if needed.</param>
    /// <returns>The added entity.</returns>
    /// <exception cref="EntityException">Thrown when an error occurs during the add operation.</exception>
    public async Task<TEntity> AddAsync(CreateSingleBuilder<TEntity> builder, CancellationToken cancellationToken = default)
    {
        try
        {
            var timeout = builder.Timeout == TimeSpan.Zero ? TimeSpan.FromSeconds(AddTimeout) : builder.Timeout;
            using var cts = new CancellationTokenSource(timeout);
            cancellationToken = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, cts.Token).Token;

            builder.Entity.CreatedBy = builder.CreatedByUser;
            await _dbSet.AddAsync(builder.Entity, cancellationToken);
            
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
            _logger.LogError(ex, "An error occurred while attempting to add an entity. {entity}, {id}", _tName.Value, builder.Entity.Id);
            throw new EntityException("An error occurred while attempting to add an entity");
        }
    }

    /// <summary>
    /// Asynchronously adds multiple entities to the repository using a configured builder.
    /// </summary>
    /// <param name="builder">Preconfigured builder containing the entities to add.</param>
    /// <param name="cancellationToken">A token to cancel the operation if needed.</param>
    /// <returns>The collection of added entities.</returns>
    /// <exception cref="EntityException">
    /// Thrown when an error occurs during the add operation or when operation is canceled.
    /// </exception>
    public async Task<IEnumerable<TEntity>> AddRangeAsync(CreateRangeBuilder<TEntity> builder, CancellationToken cancellationToken = default)
    {
        try
        {
            var timeout = builder.Timeout == TimeSpan.Zero ? TimeSpan.FromSeconds(AddRangeTimeout) : builder.Timeout;
            using var cts = new CancellationTokenSource(timeout);
            cancellationToken = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, cts.Token).Token;

            foreach (var entity in builder.Entities)
                entity.CreatedBy = builder.CreatedByUser;

            await _dbSet.AddRangeAsync(builder.Entities, cancellationToken);
            
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
            _logger.LogError(ex, "An error occurred while attempting to add a range of entities. {entity}, {identifiers}", _tName.Value, builder.Entities.Select(x =>  x.Id));
            throw new EntityException("An error occurred while attempting to add a range of entities");
        }
    }

    /// <summary>
    /// Asynchronously deletes an entity using a configured builder.
    /// </summary>
    /// <param name="builder">Builder with entity deletion parameters</param>
    /// <param name="cancellationToken">A token to cancel the operation if needed</param>
    /// <returns>The deleted entity, or null if not found</returns>
    /// <exception cref="EntityException">Thrown when an error occurs during deletion</exception>
    public async Task<TEntity?> DeleteAsync(RemoveSingleBuilder<TEntity> builder, CancellationToken cancellationToken = default)
    {
        try
        {
            var timeout = builder.Timeout == TimeSpan.Zero ? TimeSpan.FromSeconds(RemoveByFilterTimeout) : builder.Timeout;
            using var cts = new CancellationTokenSource(timeout);
            cancellationToken = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, cts.Token).Token;

            TEntity? entity = default;

            if (builder.RemoveByMode == RemoveByMode.Entity)
                entity = builder.Entity;

            if (builder.RemoveByMode == RemoveByMode.Identifier)
                entity = await _dbSet.FindAsync([builder.Id, cancellationToken], cancellationToken: cancellationToken);

            if (builder.RemoveByMode == RemoveByMode.Filter && builder.Filter is not null)
                entity = await _dbSet.FirstOrDefaultAsync(builder.Filter, cancellationToken);

            if (entity is null)
                return null;

            var deletedEntity = _dbSet.Remove(entity).Entity;
            
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
            _logger.LogError(ex, "An error occurred while attempting to delete an entity using a filter. {entity}", _tName.Value);
            throw new EntityException("An error occurred while attempting to delete an entity using a filter");
        }
    }

    /// <summary>
    /// Asynchronously deletes multiple entities using a configured builder.
    /// </summary>
    /// <param name="builder">Configured builder with deletion parameters</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Deleted entities</returns>
    /// <exception cref="EntityException">On deletion error</exception>
    public async Task<IEnumerable<TEntity>> DeleteRangeAsync(RemoveRangeBuilder<TEntity> builder, CancellationToken cancellationToken = default)
    {
        try
        {
            var timeout = builder.Timeout == TimeSpan.Zero ? TimeSpan.FromSeconds(RemoveRangeTimeout) : builder.Timeout;
            using var cts = new CancellationTokenSource(timeout);
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
                var entities = await _dbSet
                    .Where(e => builder.Identifiers.Contains(e.Id))
                    .ToArrayAsync(cancellationToken);

                _context.RemoveRange(entities);

                if (builder.SaveChanges)
                    await _context.SaveChangesAsync(cancellationToken);

                return entities;
            }

            if (builder.RemoveByMode == RemoveByMode.Filter && builder.Filter is not null)
            {
                var entities = await _dbSet
                    .Where(builder.Filter)
                    .ToArrayAsync(cancellationToken);

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
            _logger.LogError(ex, "An error occurred while attempting to delete a range of entities by their IDs. {entity}", _tName.Value);
            throw new EntityException("An error occurred while attempting to delete a range of entities by their IDs");
        }
    }

    /// <summary>
    /// Asynchronously updates an entity using a configured builder.
    /// </summary>
    /// <param name="builder">Builder with entity update parameters</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The updated entity</returns>
    /// <exception cref="EntityException">On update error</exception>
    public async Task<TEntity> UpdateAsync(UpdateSingleBuilder<TEntity> builder, CancellationToken cancellationToken = default)
    {
        try
        {
            var timeout = builder.Timeout == TimeSpan.Zero ? TimeSpan.FromSeconds(UpdateTimeout) : builder.Timeout;
            using var cts = new CancellationTokenSource(timeout);
            cancellationToken = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, cts.Token).Token;

            var entity = builder.Entity;
            entity.UpdatedBy = builder.UpdatedByUser;

            _dbSet.Attach(entity);
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
            _logger.LogError(ex, "An error occurred while attempting to update an entity. {entity}, {id}", _tName.Value, builder.Entity.Id);
            throw new EntityException("An error occurred while attempting to update an entity");
        }
    }

    /// <summary>
    /// Asynchronously updates multiple entities using a configured builder.
    /// </summary>
    /// <param name="builder">Configured builder with update parameters</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Updated entities</returns>
    /// <exception cref="EntityException">On update error</exception>
    public async Task<IEnumerable<TEntity>> UpdateRangeAsync(UpdateRangeBuilder<TEntity> builder, CancellationToken cancellationToken = default)
    {
        try
        {
            var timeout = builder.Timeout == TimeSpan.Zero ? TimeSpan.FromSeconds(UpdateRangeTimeout) : builder.Timeout;
            using var cts = new CancellationTokenSource(timeout);
            cancellationToken = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, cts.Token).Token;

            _dbSet.AttachRange(builder.Entities);

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
            _logger.LogError(ex, "An error occurred while attempting to update a range of entities. {entity}, {identifiers}", _tName.Value, builder.Entities.Select(x => x.Id));
            throw new EntityException("An error occurred while attempting to update a range of entities");
        }
    }

    /// <summary>
    /// Asynchronously restores a soft-deleted entity using a configured builder.
    /// </summary>
    /// <param name="builder">Builder with entity restore parameters</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The restored entity</returns>
    /// <exception cref="EntityException">On restore error</exception>
    public async Task<TEntity> RestoreAsync(RestoreSingleBuilder<TEntity> builder, CancellationToken cancellationToken = default)
    {
        try
        {
            var timeout = builder.Timeout == TimeSpan.Zero ? TimeSpan.FromSeconds(RestoreTimeout) : builder.Timeout;
            using var cts = new CancellationTokenSource(timeout);
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
            _logger.LogError(ex, "An error occurred while attempting to restore a soft-deleted entity, {entity}, {id}", _tName.Value, builder.Entity.Id);
            throw new EntityException("An error occurred while attempting to restore a soft-deleted entity");
        }
    }

    /// <summary>
    /// Asynchronously restores multiple soft-deleted entities using a configured builder.
    /// </summary>
    /// <param name="builder">Builder with entities to restore</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Restored entities</returns>
    /// <exception cref="EntityException">On restore error</exception>
    public async Task<IEnumerable<TEntity>> RestoreRangeAsync(RestoreRangeBuilder<TEntity> builder, CancellationToken cancellationToken = default)
    {
        try
        {
            var timeout = builder.Timeout == TimeSpan.Zero ? TimeSpan.FromSeconds(RestoreRangeTimeout) : builder.Timeout;
            using var cts = new CancellationTokenSource(timeout);
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
            _logger.LogError(ex, "An error occurred while attempting to restore a range of soft-deleted entities. {entity}, {identifiers}", _tName.Value, builder.Entities.Select(x => x.Id));
            throw new EntityException("An error occurred while attempting to restore a range of soft-deleted entities");
        }
    }
}
