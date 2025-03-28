using Infrastructure.Services.MongoDatabase.Document;
using System.Linq.Expressions;

namespace Infrastructure.Services.MongoDatabase.Builder;

/// <summary>
/// A builder class for constructing range queries with filtering, pagination and sorting
/// when querying documents from a MongoDB collection.
/// </summary>
/// <typeparam name="TDocument">
/// The document type that inherits from <see cref="DocumentBase"/>.
/// </typeparam>
/// <remarks>
/// This builder provides a fluent interface for constructing complex queries with
/// method chaining. It's immutable - each method returns a new builder instance.
/// </remarks>
public class RangeQueryDocumentBuilder<TDocument> where TDocument : DocumentBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="RangeQueryDocumentBuilder{TDocument}"/> class.
    /// </summary>
    private RangeQueryDocumentBuilder()
    {
    }

    /// <summary>
    /// Gets the filter expression used to query documents from the collection.
    /// </summary>
    public Expression<Func<TDocument, bool>>? Filter { get; private set; }

    /// <summary>
    /// Gets the number of documents to skip in the query result.
    /// </summary>
    public int Skip { get; private set; }

    /// <summary>
    /// Gets the number of documents to take (limit) in the query result.
    /// </summary>
    public int Take { get; private set; } = 1000;

    /// <summary>
    /// Gets the sorting expression for the query results.
    /// </summary>
    public Expression<Func<TDocument, object>>? OrderExpression { get; private set; }

    /// <summary>
    /// Gets whether sorting should be in descending order.
    /// </summary>
    public bool OrderByDesc { get; private set; } = true;

    /// <summary>
    /// Creates a new builder instance with default values.
    /// </summary>
    /// <returns>A new <see cref="RangeQueryDocumentBuilder{TDocument}"/> instance.</returns>
    public static RangeQueryDocumentBuilder<TDocument> Create() => new();

    /// <summary>
    /// Sets the filter expression for the query.
    /// </summary>
    /// <param name="filter">The filter expression to apply.</param>
    /// <returns>The current builder instance with updated filter.</returns>
    /// <exception cref="ArgumentNullException">Thrown when filter is null.</exception>
    public RangeQueryDocumentBuilder<TDocument> WithFilter(Expression<Func<TDocument, bool>> filter)
    {
        Filter = filter ?? throw new ArgumentNullException(nameof(filter));
        return this;
    }

    /// <summary>
    /// Sets the pagination parameters for the query.
    /// </summary>
    /// <param name="skip">Number of documents to skip.</param>
    /// <param name="take">Number of documents to take.</param>
    /// <returns>The current builder instance with updated pagination settings.</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when skip is negative or take is not positive.
    /// </exception>
    public RangeQueryDocumentBuilder<TDocument> WithPagination(int skip, int take)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(skip);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(take);

        Skip = skip;
        Take = take;
        return this;
    }

    /// <summary>
    /// Sets the sorting expression and direction for the query results.
    /// </summary>
    /// <param name="orderBy">The expression to sort by.</param>
    /// <param name="descending">Whether to sort in descending order (default: true).</param>
    /// <returns>The current builder instance with updated sorting settings.</returns>
    /// <exception cref="ArgumentNullException">Thrown when orderBy is null.</exception>
    public RangeQueryDocumentBuilder<TDocument> WithOrdering(
        Expression<Func<TDocument, object>> orderBy,
        bool descending = true)
    {
        OrderExpression = orderBy ?? throw new ArgumentNullException(nameof(orderBy));
        OrderByDesc = descending;
        return this;
    }
}

