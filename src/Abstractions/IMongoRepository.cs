using Infrastructure.Services.MongoDatabase;
using Infrastructure.Services.MongoDatabase.Builder;
using Infrastructure.Services.MongoDatabase.Document;
using System.Linq.Expressions;

namespace Infrastructure.Abstractions;

public interface IMongoRepository<TDocument> where TDocument : DocumentBase
{
    public Task<IEnumerable<TDocument>> GetRangeAsync(RangeQueryDocumentBuilder<TDocument> queryBuilder);
    public Task<TDocument?> GetByIdAsync(string id);
    public Task<TDocument?> GetByFilterAsync(Expression<Func<TDocument, bool>> query);
    public Task<string> AddAsync(TDocument documentEntity);
    public Task<IEnumerable<string>> AddRangeAsync(IEnumerable<TDocument> documentEntities);
    public Task RemoveSingleAsync(string id);
    public Task RemoveRangeAsync(IEnumerable<string> identifiers);
    public Task UpdateSingleAsync(TDocument documentEntity);
    public Task UpdateRangeAsync(IEnumerable<TDocument> documentEntities);
}

public interface IMongoRepository<TMongoContext, TDocument> where TDocument : DocumentBase where TMongoContext : MongoDatabaseContext
{
    public Task<IEnumerable<TDocument>> GetRangeAsync(RangeQueryDocumentBuilder<TDocument> queryBuilder);
    public Task<TDocument?> GetByIdAsync(string id);
    public Task<TDocument?> GetByFilterAsync(Expression<Func<TDocument, bool>> query);
    public Task<string> AddAsync(TDocument documentEntity);
    public Task<IEnumerable<string>> AddRangeAsync(IEnumerable<TDocument> documentEntities);
    public Task RemoveSingleAsync(string id);
    public Task RemoveRangeAsync(IEnumerable<string> identifiers);
    public Task UpdateSingleAsync(TDocument documentEntity);
    public Task UpdateRangeAsync(IEnumerable<TDocument> documentEntities);
}
