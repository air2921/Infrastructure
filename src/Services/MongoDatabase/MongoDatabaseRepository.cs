using Infrastructure.Abstractions;
using Infrastructure.Exceptions;
using Infrastructure.Services.MongoDatabase.Builder;
using Infrastructure.Services.MongoDatabase.Document;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using System.Linq.Expressions;

namespace Infrastructure.Services.MongoDatabase;

/// <summary>
/// A generic repository class for performing CRUD operations on a MongoDB collection.
/// This class supports working with a specific MongoDB context and document type.
/// </summary>
/// <typeparam name="TMongoContext">The type of the MongoDB context, which must inherit from <see cref="MongoDatabaseContext"/>.</typeparam>
/// <typeparam name="TDocument">The type of the document, which must inherit from <see cref="DocumentBase"/>.</typeparam>
/// <param name="context">The MongoDB context used to access the database.</param>
/// <param name="logger">A logger for tracking operations performed by this repository.</param>
/// <param name="document">An instance of the document type used to determine the collection name.</param>
/// <remarks>
/// This class provides methods for querying, inserting, updating, and deleting documents in a MongoDB collection.
/// It uses the provided <see cref="TMongoContext"/> to access the database and ensures thread-safe operations.
/// </remarks>
public class MongoDatabaseRepository<TMongoContext, TDocument>(TMongoContext context, ILogger<MongoDatabaseRepository<TMongoContext, TDocument>> logger, TDocument document)
    : IMongoRepository<TDocument>, IMongoRepository<TMongoContext, TDocument>
    where TDocument : DocumentBase
    where TMongoContext : MongoDatabaseContext
{
    private readonly Lazy<IMongoCollection<TDocument>> _collection = new(() => context.SetDocument(document), LazyThreadSafetyMode.ExecutionAndPublication);

    private static readonly Lazy<EntityException> _getRangeError = new(() => new("An error occurred while attempting to retrieve a collection of documents"), LazyThreadSafetyMode.ExecutionAndPublication);
    private static readonly Lazy<EntityException> _getByIdError = new(() => new("An error occurred while attempting to retrieve a document by its ID"), LazyThreadSafetyMode.ExecutionAndPublication);
    private static readonly Lazy<EntityException> _getByFilterError = new(() => new("An error occurred while attempting to retrieve a document using the specified filter"), LazyThreadSafetyMode.ExecutionAndPublication);
    private static readonly Lazy<EntityException> _addError = new(() => new("An error occurred while attempting to save a document"), LazyThreadSafetyMode.ExecutionAndPublication);
    private static readonly Lazy<EntityException> _addRangeError = new(() => new("An error occurred while attempting to save a collection of documents"), LazyThreadSafetyMode.ExecutionAndPublication);
    private static readonly Lazy<EntityException> _removeError = new(() => new("An error occurred while attempting to delete a document"), LazyThreadSafetyMode.ExecutionAndPublication);
    private static readonly Lazy<EntityException> _removeRangeError = new(() => new("An error occurred while attempting to delete a collection of documents"), LazyThreadSafetyMode.ExecutionAndPublication);
    private static readonly Lazy<EntityException> _updateError = new(() => new("An error occurred while attempting to update a document"), LazyThreadSafetyMode.ExecutionAndPublication);
    private static readonly Lazy<EntityException> _updateRangeError = new(() => new("An error occurred while attempting to update a collection of documents"), LazyThreadSafetyMode.ExecutionAndPublication);

    private static readonly Lazy<InsertOneOptions> _insertOneOptions = new(() => new InsertOneOptions());
    private static readonly Lazy<InsertManyOptions> _insertManyOptions = new(() => new InsertManyOptions());
    private static readonly Lazy<ReplaceOptions> _replaceOptions = new(() => new ReplaceOptions());

    /// <summary>
    /// Retrieves a range of entities from the MongoDB collection based on the provided query builder.
    /// </summary>
    /// <param name="queryBuilder">A query builder containing filtering and pagination parameters.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> that can be used to cancel the operation.</param>
    /// <returns>A collection of entities that match the query criteria.</returns>
    /// <exception cref="EntityException">Thrown if an unexpected error occurs during the operation.</exception>
    public virtual async Task<IEnumerable<TDocument>> GetRangeAsync(RangeQueryDocumentBuilder<TDocument> queryBuilder, CancellationToken cancellationToken = default)
    {
        try
        {
            logger.LogInformation($"Received request to get range of entities\nCollection name: {document.CollectionName}");

            var findOptions = new FindOptions<TDocument>
            {
                Skip = queryBuilder.Skip,
                Limit = queryBuilder.Take
            };

            var findFluent = _collection.Value.Find(
                queryBuilder.Filter ?? Builders<TDocument>.Filter.Empty);

            if (queryBuilder.OrderExpression is not null)
            {
                findFluent = queryBuilder.OrderByDesc
                    ? findFluent.Sort(Builders<TDocument>.Sort.Descending(queryBuilder.OrderExpression))
                    : findFluent.Sort(Builders<TDocument>.Sort.Ascending(queryBuilder.OrderExpression));
            }

            return await findFluent
                .Skip(queryBuilder.Skip)
                .Limit(queryBuilder.Take)
                .ToListAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex.ToString());
            throw _getRangeError.Value;
        }
    }

    /// <summary>
    /// Retrieves an entity from the collection by its unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the entity.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> that can be used to cancel the operation.</param>
    /// <returns>The entity with the specified identifier, or <c>null</c> if no such entity exists.</returns>
    /// <exception cref="EntityException">Thrown if an unexpected error occurs during the operation.</exception>
    public virtual async Task<TDocument?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        try
        {
            logger.LogInformation($"Received request to get entity by id {id}\nCollection name: {document.CollectionName}");

            return await _collection.Value.Find(x => x.Id.Equals(id)).FirstOrDefaultAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex.ToString());
            throw _getByIdError.Value;
        }
    }

    /// <summary>
    /// Retrieves an entity from the collection based on a specified filter.
    /// </summary>
    /// <param name="query">A filter expression to apply to the collection.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> that can be used to cancel the operation.</param>
    /// <returns>The first entity that matches the filter, or <c>null</c> if no such entity exists.</returns>
    /// <exception cref="EntityException">Thrown if an unexpected error occurs during the operation.</exception>
    public virtual async Task<TDocument?> GetByFilterAsync(Expression<Func<TDocument, bool>> query, CancellationToken cancellationToken = default)
    {
        try
        {
            logger.LogInformation($"Received request to get entity by filter\nCollection name: {document.CollectionName}");

            return await _collection.Value.Find(query).FirstOrDefaultAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex.ToString());
            throw _getByFilterError.Value;
        }
    }

    /// <summary>
    /// Adds a single entity to the collection.
    /// </summary>
    /// <param name="documentEntity">The entity to add to the collection.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> that can be used to cancel the operation.</param>
    /// <returns>The unique identifier of the added entity.</returns>
    /// <exception cref="EntityException">Thrown if an unexpected error occurs during the operation.</exception>
    public virtual async Task<string> AddAsync(TDocument documentEntity, CancellationToken cancellationToken = default)
    {
        try
        {
            logger.LogInformation($"Received request to insert one entity\nCollection name: {document.CollectionName}");

            await _collection.Value.InsertOneAsync(document, _insertOneOptions.Value, cancellationToken);
            return documentEntity.Id;
        }
        catch (Exception ex)
        {
            logger.LogError(ex.ToString());
            throw _addError.Value;
        }
    }

    /// <summary>
    /// Adds a collection of entities to the MongoDB collection.
    /// </summary>
    /// <param name="documentEntities">The collection of entities to add.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> that can be used to cancel the operation.</param>
    /// <returns>A collection of unique identifiers for the added entities.</returns>
    /// <exception cref="EntityException">Thrown if an unexpected error occurs during the operation.</exception>
    public virtual async Task<IEnumerable<string>> AddRangeAsync(IEnumerable<TDocument> documentEntities, CancellationToken cancellationToken = default)
    {
        try
        {
            logger.LogInformation($"Received request to insert a range of entities\nCollection name: {document.CollectionName}");

            var task = _collection.Value.InsertManyAsync(documentEntities, _insertManyOptions.Value, cancellationToken);

            var identifiers = new HashSet<string>();
            identifiers.UnionWith(documentEntities.Select(x => x.Id));

            await task;
            return identifiers;
        }
        catch (Exception ex)
        {
            logger.LogError(ex.ToString());
            throw _addRangeError.Value;
        }
    }

    /// <summary>
    /// Removes a single entity from the collection by its unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the entity to remove.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> that can be used to cancel the operation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    /// <exception cref="EntityException">Thrown if an unexpected error occurs during the operation.</exception>
    public virtual async Task RemoveSingleAsync(string id, CancellationToken cancellationToken = default)
    {
        try
        {
            logger.LogInformation($"Received request to remove one entity by id {id}\nCollection name: {document.CollectionName}");

            await _collection.Value.DeleteOneAsync(x => x.Id == id, cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex.ToString());
            throw _removeError.Value;
        }
    }

    /// <summary>
    /// Removes a collection of entities from the MongoDB collection by their unique identifiers.
    /// </summary>
    /// <param name="identifiers">The collection of unique identifiers of the entities to remove.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> that can be used to cancel the operation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    /// <exception cref="EntityException">Thrown if an unexpected error occurs during the operation.</exception>
    public virtual async Task RemoveRangeAsync(IEnumerable<string> identifiers, CancellationToken cancellationToken = default)
    {
        try
        {
            logger.LogInformation($"Received request to remove a range of entities by identifiers\nCollection name: {document.CollectionName}");

            var tasks = identifiers.Select(id => _collection.Value.DeleteOneAsync(x => x.Id.Equals(id), cancellationToken));
            await Task.WhenAll(tasks);
        }
        catch (Exception ex)
        {
            logger.LogError(ex.ToString());
            throw _removeRangeError.Value;
        }
    }

    /// <summary>
    /// Updates a single entity in the MongoDB collection.
    /// </summary>
    /// <param name="documentEntity">The entity to update.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> that can be used to cancel the operation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    /// <exception cref="EntityException">Thrown if an unexpected error occurs during the operation.</exception>
    public virtual async Task UpdateSingleAsync(TDocument documentEntity, CancellationToken cancellationToken = default)
    {
        try
        {
            logger.LogInformation($"Received request to update one entity, entityId {documentEntity.Id}\nCollection name: {document.CollectionName}");

            await _collection.Value.ReplaceOneAsync(x => x.Id.Equals(documentEntity.Id), documentEntity, _replaceOptions.Value, cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex.ToString());
            throw _updateError.Value;
        }
    }

    /// <summary>
    /// Updates a collection of entities in the MongoDB collection.
    /// </summary>
    /// <param name="documentEntities">The collection of entities to update.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> that can be used to cancel the operation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    /// <exception cref="EntityException">Thrown if an unexpected error occurs during the operation.</exception>
    public virtual async Task UpdateRangeAsync(IEnumerable<TDocument> documentEntities, CancellationToken cancellationToken = default)
    {
        try
        {
            logger.LogInformation($"Received request to update a range of entities\nCollection name: {document.CollectionName}");

            var tasks = documentEntities.Select(documentEntity => _collection.Value.ReplaceOneAsync(x => x.Id.Equals(documentEntity.Id), documentEntity, _replaceOptions.Value, cancellationToken));
            await Task.WhenAll(tasks);
        }
        catch (Exception ex)
        {
            logger.LogError(ex.ToString());
            throw _updateRangeError.Value;
        }
    }
}
