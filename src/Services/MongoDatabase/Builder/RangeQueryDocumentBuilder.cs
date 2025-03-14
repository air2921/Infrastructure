using Infrastructure.Services.MongoDatabase.Document;
using System.Linq.Expressions;

namespace Infrastructure.Services.MongoDatabase.Builder;

/// <summary>
/// A class acting as a DTO (Data Transfer Object) for passing sorting, filtering, and pagination data
/// when querying entities from a MongoDb collection.
/// </summary>
/// <typeparam name="TDocument">
/// The base class for all entities in MongoDb.
/// All entities that should be stored in MongoDb must inherit from <see cref="DocumentBase"/> and implement it.
/// </typeparam>
public class RangeQueryDocumentBuilder<TDocument> where TDocument : DocumentBase
{
    /// <summary>
    /// Gets or sets the filter expression used to query documents from the collection.
    /// The filter is applied to the collection to retrieve only the documents that match the specified criteria.
    /// </summary>
    /// <example>
    /// <code>
    /// builder.Filter = x => x.Name == "John";
    /// </code>
    /// </example>
    public Expression<Func<TDocument, bool>>? Filter { get; set; }

    /// <summary>
    /// Gets or sets the number of documents to skip in the query result.
    /// This is typically used for pagination to skip a certain number of documents.
    /// </summary>
    /// <example>
    /// <code>
    /// builder.Skip = 10; // Skips the first 10 documents.
    /// </code>
    /// </example>
    public int Skip { get; set; }

    /// <summary>
    /// Gets or sets the number of documents to take (limit) in the query result.
    /// This is typically used for pagination to limit the number of documents returned.
    /// </summary>
    /// <example>
    /// <code>
    /// builder.Take = 20; // Returns up to 20 documents.
    /// </code>
    /// </example>
    public int Take { get; set; }
}

