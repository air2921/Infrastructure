using Infrastructure.Exceptions;
using Infrastructure.Services.MongoDatabase;
using Infrastructure.Services.MongoDatabase.Builder;
using Infrastructure.Services.MongoDatabase.Document;
using System.Linq.Expressions;

namespace Infrastructure.Abstractions;

/// <summary>
/// Represents a repository for interacting with MongoDB for a specific document type.
/// </summary>
/// <typeparam name="TDocument">The type of document the repository will handle, which must inherit from <see cref="DocumentBase"/>.</typeparam>
public interface IMongoRepository<TDocument> where TDocument : DocumentBase
{
    /// <summary>
    /// Retrieves a range of documents based on the provided query builder.
    /// </summary>
    /// <param name="queryBuilder">The query builder used to construct the query.</param>
    /// <param name="cancellationToken">The cancellation token to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation, with a collection of documents as the result.</returns>
    /// <exception cref="EntityException">Thrown when an error occurs while retrieving the documents.</exception>
    public Task<IEnumerable<TDocument>> GetRangeAsync(RangeQueryDocumentBuilder<TDocument> queryBuilder, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a document by its identifier.
    /// </summary>
    /// <param name="id">The identifier of the document.</param>
    /// <param name="cancellationToken">The cancellation token to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation, with the document as the result, or <c>null</c> if not found.</returns>
    /// <exception cref="EntityException">Thrown when an error occurs while retrieving the document.</exception>
    public Task<TDocument?> GetByIdAsync(string id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a document based on a specified filter.
    /// </summary>
    /// <param name="query">The filter expression used to find the document.</param>
    /// <param name="cancellationToken">The cancellation token to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation, with the document as the result, or <c>null</c> if not found.</returns>
    /// <exception cref="EntityException">Thrown when an error occurs while retrieving the document.</exception>
    public Task<TDocument?> GetByFilterAsync(Expression<Func<TDocument, bool>> query, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a new document to the collection.
    /// </summary>
    /// <param name="documentEntity">The document to add.</param>
    /// <param name="cancellationToken">The cancellation token to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation, with the identifier of the added document.</returns>
    /// <exception cref="EntityException">Thrown when an error occurs while adding the document.</exception>
    public Task<string> AddAsync(TDocument documentEntity, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds multiple documents to the collection.
    /// </summary>
    /// <param name="documentEntities">The documents to add.</param>
    /// <param name="cancellationToken">The cancellation token to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation, with the identifiers of the added documents.</returns>
    /// <exception cref="EntityException">Thrown when an error occurs while adding the documents.</exception>
    public Task<IEnumerable<string>> AddRangeAsync(IEnumerable<TDocument> documentEntities, CancellationToken cancellationToken = default);

    /// <summary>
    /// Removes a document based on its identifier.
    /// </summary>
    /// <param name="id">The identifier of the document to remove.</param>
    /// <param name="cancellationToken">The cancellation token to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    /// <exception cref="EntityException">Thrown when an error occurs while removing the document.</exception>
    public Task RemoveSingleAsync(string id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Removes multiple documents based on their identifiers.
    /// </summary>
    /// <param name="identifiers">The identifiers of the documents to remove.</param>
    /// <param name="cancellationToken">The cancellation token to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    /// <exception cref="EntityException">Thrown when an error occurs while removing the documents.</exception>
    public Task RemoveRangeAsync(IEnumerable<string> identifiers, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates a single document in the collection.
    /// </summary>
    /// <param name="documentEntity">The document with updated data.</param>
    /// <param name="cancellationToken">The cancellation token to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    /// <exception cref="EntityException">Thrown when an error occurs while updating the document.</exception>
    public Task UpdateSingleAsync(TDocument documentEntity, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates multiple documents in the collection.
    /// </summary>
    /// <param name="documentEntities">The documents with updated data.</param>
    /// <param name="cancellationToken">The cancellation token to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    /// <exception cref="EntityException">Thrown when an error occurs while updating the documents.</exception>
    public Task UpdateRangeAsync(IEnumerable<TDocument> documentEntities, CancellationToken cancellationToken = default);
}

/// <summary>
/// Represents a repository for interacting with MongoDB for a specific document type and a specific context.
/// </summary>
/// <typeparam name="TMongoContext">The type of the MongoDB context, which must inherit from <see cref="MongoDatabaseContext"/>.</typeparam>
/// <typeparam name="TDocument">The type of document the repository will handle, which must inherit from <see cref="DocumentBase"/>.</typeparam>
public interface IMongoRepository<TMongoContext, TDocument> : 
    IMongoRepository<TDocument>
    where TDocument : DocumentBase
    where TMongoContext : MongoDatabaseContext
{

}
