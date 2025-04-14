using Infrastructure.Abstractions.Database;
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
/// <typeparam name="TMongoContext">The type of the MongoDB context, which must inherit from <see cref="MongoContext"/>.</typeparam>
/// <typeparam name="TDocument">The type of the document, which must inherit from <see cref="DocumentBase"/>.</typeparam>
/// <remarks>
/// This class provides methods for querying, inserting, updating, and deleting documents in a MongoDB collection.
/// It uses the provided <typeparamref name="TMongoContext"/> to access the database and ensures thread-safe operations.
/// </remarks>
public sealed class MongoDatabaseRepository<TMongoContext, TDocument>(TMongoContext context, ILogger<MongoDatabaseRepository<TMongoContext, TDocument>> logger, TDocument document)
    : IMongoRepository<TDocument>, IMongoRepository<TMongoContext, TDocument>
    where TDocument : DocumentBase
    where TMongoContext : MongoContext
{
    private readonly Lazy<IMongoCollection<TDocument>> _collection = new(() => context.SetDocument(document), LazyThreadSafetyMode.ExecutionAndPublication);

    private static readonly Lazy<InsertOneOptions> _insertOneOptions = new(() => new InsertOneOptions());
    private static readonly Lazy<InsertManyOptions> _insertManyOptions = new(() => new InsertManyOptions());
    private static readonly Lazy<DeleteOptions> _deleteOptions = new(() => new DeleteOptions());

    /// <summary>
    /// Retrieves a range of entities from the MongoDB collection based on the provided query builder.
    /// </summary>
    /// <param name="queryBuilder">A query builder containing filtering and pagination parameters.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> that can be used to cancel the operation.</param>
    /// <returns>A collection of entities that match the query criteria.</returns>
    /// <exception cref="EntityException">Thrown if an unexpected error occurs during the operation.</exception>
    public async Task<IEnumerable<TDocument>> GetRangeAsync(RangeQueryDocumentBuilder<TDocument> queryBuilder, CancellationToken cancellationToken = default)
    {
        try
        {
            logger.LogInformation("Received request to get range of entities", [document.CollectionName]);

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
            logger.LogError(ex, "An error occurred while attempting to retrieve a collection of documents");
            throw new EntityException("An error occurred while attempting to retrieve a collection of documents");
        }
    }

    /// <summary>
    /// Retrieves an entity from the collection by its unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the entity.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> that can be used to cancel the operation.</param>
    /// <returns>The entity with the specified identifier, or <c>null</c> if no such entity exists.</returns>
    /// <exception cref="EntityException">Thrown if an unexpected error occurs during the operation.</exception>
    public async Task<TDocument?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        try
        {
            logger.LogInformation("Received request to get entity by id. {collection}, {id}", document.CollectionName, id);

            return await _collection.Value.Find(x => x.Id.Equals(id)).FirstOrDefaultAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while attempting to retrieve a document by its ID, {id}", id);
            throw new EntityException("An error occurred while attempting to retrieve a document by its ID");
        }
    }

    /// <summary>
    /// Retrieves an entity from the collection based on a specified filter.
    /// </summary>
    /// <param name="query">A filter expression to apply to the collection.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> that can be used to cancel the operation.</param>
    /// <returns>The first entity that matches the filter, or <c>null</c> if no such entity exists.</returns>
    /// <exception cref="EntityException">Thrown if an unexpected error occurs during the operation.</exception>
    public async Task<TDocument?> GetByFilterAsync(Expression<Func<TDocument, bool>> query, CancellationToken cancellationToken = default)
    {
        try
        {
            logger.LogInformation("Received request to get entity by filter. {collection}", document.CollectionName);

            return await _collection.Value.Find(query).FirstOrDefaultAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while attempting to retrieve a document using the specified filter");
            throw new EntityException("An error occurred while attempting to retrieve a document using the specified filter");
        }
    }

    /// <summary>
    /// Adds a single entity to the collection.
    /// </summary>
    /// <param name="documentEntity">The entity to add to the collection.</param>
    /// <param name="sessionHandle">Optional MongoDB client session handle for transaction support. 
    /// If null, the operation will be executed without a transaction.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> that can be used to cancel the operation.</param>
    /// <returns>The unique identifier of the added entity.</returns>
    /// <exception cref="EntityException">Thrown if an unexpected error occurs during the operation.</exception>
    public async Task<string> AddAsync(TDocument documentEntity, IClientSessionHandle? sessionHandle = null, CancellationToken cancellationToken = default)
    {
        try
        {
            logger.LogInformation("Received request to insert one entity. {collection}", document.CollectionName);

            if (sessionHandle is not null)
                await _collection.Value.InsertOneAsync(sessionHandle, documentEntity, _insertOneOptions.Value, cancellationToken);
            else
                await _collection.Value.InsertOneAsync(documentEntity, _insertOneOptions.Value, cancellationToken);

            return documentEntity.Id;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while attempting to save a document. {collection}, {id}", documentEntity.CollectionName, documentEntity.Id);
            throw new EntityException("An error occurred while attempting to save a document");
        }
    }

    /// <summary>
    /// Adds a collection of entities to the MongoDB collection.
    /// </summary>
    /// <param name="documentEntities">The collection of entities to add.</param>
    /// <param name="sessionHandle">Optional MongoDB client session handle for transaction support.
    /// If null, the operation will be executed without a transaction.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> that can be used to cancel the operation.</param>
    /// <returns>A collection of unique identifiers for the added entities.</returns>
    /// <exception cref="EntityException">Thrown if an unexpected error occurs during the operation.</exception>
    public async Task<IEnumerable<string>> AddRangeAsync(IEnumerable<TDocument> documentEntities, IClientSessionHandle? sessionHandle = null, CancellationToken cancellationToken = default)
    {
        try
        {
            logger.LogInformation("Received request to insert a range of entities. {collection}, {identifiers}", document.CollectionName, documentEntities.Select(x => x.Id));

            if (sessionHandle is not null)
                await _collection.Value.InsertManyAsync(sessionHandle, documentEntities, _insertManyOptions.Value, cancellationToken);
            else
                await _collection.Value.InsertManyAsync(documentEntities, _insertManyOptions.Value, cancellationToken);

            return documentEntities.Select(x => x.Id).ToHashSet();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while attempting to save a collection of documents. {collection}, {identifiers}", document.CollectionName, documentEntities.Select(x => x.Id));
            throw new EntityException("An error occurred while attempting to save a collection of documents");
        }
    }

    /// <summary>
    /// Removes a single entity from the collection by its unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the entity to remove.</param>
    /// <param name="sessionHandle">Optional MongoDB client session handle for transaction support.
    /// If null, the operation will be executed without a transaction.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> that can be used to cancel the operation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    /// <exception cref="EntityException">Thrown if an unexpected error occurs during the operation.</exception>
    public async Task RemoveSingleAsync(string id, IClientSessionHandle? sessionHandle = null, CancellationToken cancellationToken = default)
    {
        try
        {
            logger.LogInformation("Received request to remove one entity by id. {collection}, {id}", document.CollectionName, id);

            var filter = Builders<TDocument>.Filter.Eq(x => x.Id, id);

            if (sessionHandle is not null)
                await _collection.Value.DeleteOneAsync(sessionHandle, filter, cancellationToken: cancellationToken);
            else
                await _collection.Value.DeleteOneAsync(filter, cancellationToken: cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while attempting to delete a document. {collection}, {id}", document.CollectionName, id);
            throw new EntityException("An error occurred while attempting to delete a document");
        }
    }

    /// <summary>
    /// Removes a collection of entities from the MongoDB collection by their unique identifiers.
    /// </summary>
    /// <param name="identifiers">The collection of unique identifiers of the entities to remove.</param>
    /// <param name="sessionHandle">Optional MongoDB client session handle for transaction support.
    /// If null, the operation will be executed without a transaction.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> that can be used to cancel the operation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    /// <exception cref="EntityException">Thrown if an unexpected error occurs during the operation.</exception>
    public async Task RemoveRangeAsync(IEnumerable<string> identifiers, IClientSessionHandle? sessionHandle = null, CancellationToken cancellationToken = default)
    {
        try
        {
            logger.LogInformation("Received request to remove a range of entities by identifiers. {collection}, {identifiers}", document.CollectionName, identifiers);

            var filter = Builders<TDocument>.Filter.In(x => x.Id, identifiers);

            if (sessionHandle is not null)
                await _collection.Value.DeleteManyAsync(sessionHandle, filter, _deleteOptions.Value, cancellationToken);
            else
                await _collection.Value.DeleteManyAsync(filter, cancellationToken);

        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while attempting to delete a collection of documents. {collection}, {identifiers}", document.CollectionName, identifiers);
            throw new EntityException("An error occurred while attempting to delete a collection of documents");
        }
    }

    /// <summary>
    /// Updates a single entity in the MongoDB collection.
    /// </summary>
    /// <param name="documentEntity">The entity to update.</param>
    /// <param name="sessionHandle">Optional MongoDB client session handle for transaction support.
    /// If null, the operation will be executed without a transaction.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> that can be used to cancel the operation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    /// <exception cref="EntityException">Thrown if an unexpected error occurs during the operation.</exception>
    public async Task UpdateSingleAsync(TDocument documentEntity, IClientSessionHandle? sessionHandle = null, CancellationToken cancellationToken = default)
    {
        try
        {
            logger.LogInformation("Received request to update one entity. {collection}, {id}", document.CollectionName, documentEntity.Id);

            document.UpdatedAt = DateTimeOffset.UtcNow;
            var filter = Builders<TDocument>.Filter.Eq(x => x.Id, documentEntity.Id);

            if (sessionHandle is not null)
                await _collection.Value.ReplaceOneAsync(sessionHandle, filter, documentEntity, cancellationToken: cancellationToken);
            else
                await _collection.Value.ReplaceOneAsync(filter, documentEntity, cancellationToken: cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while attempting to update a document. {collection}, {id}", document.CollectionName, documentEntity.Id);
            throw new EntityException("An error occurred while attempting to update a document");
        }
    }

    /// <summary>
    /// Updates a collection of entities in the MongoDB collection using bulk operations.
    /// </summary>
    /// <param name="documentEntities">The collection of entities to update.</param>
    /// <param name="sessionHandle">Optional MongoDB client session handle for transaction support.
    /// If null, the operation will be executed without a transaction.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> that can be used to cancel the operation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    /// <exception cref="EntityException">Thrown when:
    /// <list type="bullet">
    /// <item><description>The input collection is null</description></item>
    /// <item><description>No documents were provided for update</description></item>
    /// <item><description>An unexpected error occurs during the bulk operation</description></item>
    /// </list>
    /// </exception>
    public async Task UpdateRangeAsync(IEnumerable<TDocument> documentEntities, IClientSessionHandle? sessionHandle = null, CancellationToken cancellationToken = default)
    {
        try
        {
            logger.LogInformation("Received request to update a range of entities. {collection}, {identifiers}", document.CollectionName, documentEntities.Select(x => x.Id));

            var documentsList = documentEntities.ToList();
            var bulkOps = new List<WriteModel<TDocument>>();

            foreach (var doc in documentsList)
            {
                doc.UpdatedAt = DateTimeOffset.UtcNow;
                var filter = Builders<TDocument>.Filter.Eq(x => x.Id, doc.Id);
                bulkOps.Add(new ReplaceOneModel<TDocument>(filter, doc));
            }

            if (bulkOps.Count > 0)
            {
                if (sessionHandle is not null)
                    await _collection.Value.BulkWriteAsync(sessionHandle, bulkOps, cancellationToken: cancellationToken);
                else
                    await _collection.Value.BulkWriteAsync(bulkOps, cancellationToken: cancellationToken);
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while attempting to update a collection of documents. {collection}, {identifiers}", document.CollectionName, documentEntities.Select(x => x.Id));
            throw new EntityException("An error occurred while attempting to update a collection of documents");
        }
    }
}
