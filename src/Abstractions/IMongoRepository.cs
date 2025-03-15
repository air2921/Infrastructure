using Infrastructure.Services.MongoDatabase;
using Infrastructure.Services.MongoDatabase.Builder;
using Infrastructure.Services.MongoDatabase.Document;
using System.Linq.Expressions;

namespace Infrastructure.Abstractions;

public interface IMongoRepository<TDocument> where TDocument : DocumentBase
{
    public Task<IEnumerable<TDocument>> GetRangeAsync(RangeQueryDocumentBuilder<TDocument> queryBuilder, CancellationToken cancellationToken = default);
    public Task<TDocument?> GetByIdAsync(string id, CancellationToken cancellationToken = default);
    public Task<TDocument?> GetByFilterAsync(Expression<Func<TDocument, bool>> query, CancellationToken cancellationToken = default);
    public Task<string> AddAsync(TDocument documentEntity, CancellationToken cancellationToken = default);
    public Task<IEnumerable<string>> AddRangeAsync(IEnumerable<TDocument> documentEntities, CancellationToken cancellationToken = default);
    public Task RemoveSingleAsync(string id, CancellationToken cancellationToken = default);
    public Task RemoveRangeAsync(IEnumerable<string> identifiers, CancellationToken cancellationToken = default);
    public Task UpdateSingleAsync(TDocument documentEntity, CancellationToken cancellationToken = default);
    public Task UpdateRangeAsync(IEnumerable<TDocument> documentEntities, CancellationToken cancellationToken = default);
    public Task BeginTransactionAsync(CancellationToken cancellationToken = default);
    public Task CommitTransactionAsync(CancellationToken cancellationToken = default);
    public Task RollbackTransactionAsync(CancellationToken cancellationToken = default);
    public void BeginTransaction();
    public void CommitTransaction();
    public void RollbackTransaction();
}

public interface IMongoRepository<TMongoContext, TDocument> where TDocument : DocumentBase where TMongoContext : MongoDatabaseContext
{
    public Task<IEnumerable<TDocument>> GetRangeAsync(RangeQueryDocumentBuilder<TDocument> queryBuilder, CancellationToken cancellationToken = default);
    public Task<TDocument?> GetByIdAsync(string id, CancellationToken cancellationToken = default);
    public Task<TDocument?> GetByFilterAsync(Expression<Func<TDocument, bool>> query, CancellationToken cancellationToken = default);
    public Task<string> AddAsync(TDocument documentEntity, CancellationToken cancellationToken = default);
    public Task<IEnumerable<string>> AddRangeAsync(IEnumerable<TDocument> documentEntities, CancellationToken cancellationToken = default);
    public Task RemoveSingleAsync(string id, CancellationToken cancellationToken = default);
    public Task RemoveRangeAsync(IEnumerable<string> identifiers, CancellationToken cancellationToken = default);
    public Task UpdateSingleAsync(TDocument documentEntity, CancellationToken cancellationToken = default);
    public Task UpdateRangeAsync(IEnumerable<TDocument> documentEntities, CancellationToken cancellationToken = default);
    public Task BeginTransactionAsync(CancellationToken cancellationToken = default);
    public Task CommitTransactionAsync(CancellationToken cancellationToken = default);
    public Task RollbackTransactionAsync(CancellationToken cancellationToken = default);
    public void BeginTransaction();
    public void CommitTransaction();
    public void RollbackTransaction();
}
