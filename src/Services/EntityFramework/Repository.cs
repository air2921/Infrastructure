using Infrastructure.Abstractions;
using Infrastructure.Exceptions;
using Infrastructure.Services.EntityFramework.Builder;
using Infrastructure.Services.EntityFramework.Entity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Linq.Expressions;

namespace Infrastructure.Services.EntityFramework;

public class Repository<TEntity, TDbContext> :
    IRepository<TEntity>,
    IRepository<TEntity, TDbContext> where TEntity : EntityBase where TDbContext : DbContext
{
    #region fields and constructor

    private readonly ILogger<Repository<TEntity, TDbContext>> _logger;
    private readonly TDbContext _context;
    private readonly DbSet<TEntity> _dbSet;
    private readonly string _tName = typeof(TEntity).FullName ?? typeof(TEntity).Name;

    /// <summary>
    /// Constructor that sets up the DbSet for the entity T.
    /// </summary>
    /// <param name="logger">Logger for tracking repository operations.</param>
    /// <param name="context">EFCore context.</param>
    /// <exception cref="ArgumentNullException">Thrown if context is null.</exception>
    public Repository(ILogger<Repository<TEntity, TDbContext>> logger, TDbContext context)
    {
        _logger = logger;
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _dbSet = _context.Set<TEntity>();
    }

    #endregion

    /// <summary>
    /// Gets an IQueryable<T> for setting up a query to the database for T.
    /// It is intended to be used only for configuring the loading of related entities.
    /// </summary>
    /// <returns>IQueryable<T></returns>
    public virtual IQueryable<TEntity> GetQuery() => _dbSet;

    public ValueTask<int> GetCountAsync(Expression<Func<TEntity, bool>>? filter)
    {
        try
        {
            IQueryable<TEntity> query = _dbSet;
            if (filter is not null)
                query = query.Where(filter);

            var result = query.Count();
            return new ValueTask<int>(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex.ToString(), _tName);
            throw new EntityException();
        }
    }

    public async Task<TEntity?> GetByIdAsync(object id, CancellationToken cancellationToken = default)
    {
        try
        {
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(Immutable.GetByIdAwait));
            cancellationToken = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, cts.Token).Token;

            return await _dbSet.FindAsync([id, cancellationToken], cancellationToken: cancellationToken);
        }
        catch (OperationCanceledException)
        {
            throw new EntityException();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex.ToString(), _tName);
            throw new EntityException();
        }
    }

    public async Task<TEntity?> GetByFilterAsync(Expression<Func<TEntity, bool>> filter, CancellationToken cancellationToken = default)
    {
        try
        {
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(Immutable.GetByFilterAwait));
            cancellationToken = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, cts.Token).Token;

            IQueryable<TEntity> query = _dbSet;

            query = query.AsNoTracking();
            query = query.Where(filter);

            return await query.FirstOrDefaultAsync(cancellationToken);
        }
        catch (OperationCanceledException)
        {
            throw new EntityException();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex.ToString(), _tName);
            throw new EntityException();
        }
    }

    public async Task<TEntity?> GetByFilterAsync(SingleQueryBuilder<TEntity> builder, CancellationToken cancellationToken = default)
    {
        try
        {
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(Immutable.GetByFilterAwait));
            cancellationToken = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, cts.Token).Token;

            IQueryable<TEntity> query = _dbSet;

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
            throw new EntityException();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex.ToString(), _tName);
            throw new EntityException();
        }
    }

    public async Task<IEnumerable<TEntity>> GetRangeAsync(RangeQueryBuilder<TEntity>? builder, CancellationToken cancellationToken = default)
    {
        try
        {
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(Immutable.GetRangeAwait));
            cancellationToken = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, cts.Token).Token;

            IQueryable<TEntity> query = _dbSet;

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
            throw new EntityException();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex.ToString(), _tName);
            throw new EntityException();
        }
    }

    public async Task<TEntity> AddAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        try
        {
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(Immutable.AddAwait));
            cancellationToken = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, cts.Token).Token;

            await _dbSet.AddAsync(entity, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);

            return entity;
        }
        catch (OperationCanceledException)
        {
            throw new EntityException();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex.ToString(), _tName);
            throw new EntityException();
        }
    }

    public async Task AddRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default)
    {
        try
        {
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(Immutable.AddRangeAwait));
            cancellationToken = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, cts.Token).Token;

            await _dbSet.AddRangeAsync(entities, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
        }
        catch (OperationCanceledException)
        {
            throw new EntityException();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex.ToString(), _tName);
            throw new EntityException();
        }
    }

    public async Task<TEntity?> DeleteByIdAsync(object id, CancellationToken cancellationToken = default)
    {
        try
        {
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(Immutable.RemoveByIdAwait));
            cancellationToken = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, cts.Token).Token;

            var entity = await _dbSet.FindAsync([id, cancellationToken], cancellationToken: cancellationToken);
            if (entity is not null)
            {
                var deletedEntity = _dbSet.Remove(entity).Entity;
                await _context.SaveChangesAsync(cancellationToken);
                return deletedEntity;
            }
            return null;
        }
        catch (OperationCanceledException)
        {
            throw new EntityException();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex.ToString(), _tName);
            throw new EntityException();
        }
    }

    public async Task<TEntity?> DeleteByFilterAsync(Expression<Func<TEntity, bool>> filter, CancellationToken cancellationToken = default)
    {
        try
        {
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(Immutable.RemoveByFilterAwait));
            cancellationToken = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, cts.Token).Token;

            IQueryable<TEntity> query = _dbSet;
            query = query.Where(filter);

            var entity = await query.FirstOrDefaultAsync(cancellationToken);
            if (entity is null)
                return null;

            var deletedEntity = _dbSet.Remove(entity).Entity;
            await _context.SaveChangesAsync(cancellationToken);
            return deletedEntity;
        }
        catch (OperationCanceledException)
        {
            throw new EntityException();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex.ToString(), _tName);
            throw new EntityException();
        }
    }

    public async Task<IEnumerable<TEntity>> DeleteRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default)
    {
        try
        {
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(Immutable.RemoveRangeAwait));
            cancellationToken = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, cts.Token).Token;

            _dbSet.RemoveRange(entities);
            await _context.SaveChangesAsync(cancellationToken);
            return entities;
        }
        catch (OperationCanceledException)
        {
            throw new EntityException();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex.ToString(), _tName);
            throw new EntityException();
        }
    }

    public async Task<IEnumerable<TEntity>> DeleteRangeAsync(IEnumerable<object> identifiers, CancellationToken cancellationToken = default)
    {
        try
        {
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(Immutable.RemoveRangeAwait));
            cancellationToken = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, cts.Token).Token;

            var entities = new List<TEntity>();

            foreach (var id in identifiers)
            {
                var entity = await _dbSet.FindAsync([id, cancellationToken], cancellationToken: cancellationToken);
                if (entity is not null)
                    entities.Add(entity);
            }

            _dbSet.RemoveRange(entities);
            await _context.SaveChangesAsync(cancellationToken);
            return entities;
        }
        catch (OperationCanceledException)
        {
            throw new EntityException();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex.ToString(), _tName);
            throw new EntityException();
        }
    }

    public async Task<TEntity?> UpdateAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        try
        {
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(Immutable.UpdateAwait));
            cancellationToken = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, cts.Token).Token;

            _dbSet.Attach(entity);
            _context.Entry(entity).State = EntityState.Modified;

            await _context.SaveChangesAsync(cancellationToken);
            return entity;
        }
        catch (OperationCanceledException)
        {
            throw new EntityException();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex.ToString(), _tName);
            throw new EntityException();
        }
    }

    public async Task<IEnumerable<TEntity?>> UpdateRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default)
    {
        try
        {
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(Immutable.UpdateRangeAwait));
            cancellationToken = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, cts.Token).Token;

            foreach (var entity in entities)
            {
                _dbSet.Attach(entity);
                _context.Entry(entity).State = EntityState.Modified;
            }

            await _context.SaveChangesAsync(cancellationToken);
            return entities;
        }
        catch (OperationCanceledException)
        {
            throw new EntityException();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex.ToString(), _tName);
            throw new EntityException();
        }
    }
}
