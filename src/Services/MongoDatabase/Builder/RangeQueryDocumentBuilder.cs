using Infrastructure.Exceptions;
using Infrastructure.Services.EntityFramework.Builder;
using Infrastructure.Services.MongoDatabase.Document;
using System.ComponentModel;
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
public sealed class RangeQueryDocumentBuilder<TDocument> where TDocument : DocumentBase
{
    /// <summary>
    /// A flag indicating whether to ignore builder constraints (like maximum take limit).
    /// This should be used with caution as it bypasses safety checks.
    /// </summary>
    private bool ignoreBuilderConstraints = false;

    /// <summary>
    /// Private constructor to enforce use of factory method.
    /// </summary>
    private RangeQueryDocumentBuilder()
    {
    }

    /// <summary>
    /// Gets the filter expression used to query documents from the collection.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    internal Expression<Func<TDocument, bool>>? Filter { get; private set; }

    /// <summary>
    /// Gets the number of documents to skip in the query result.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    internal int Skip { get; private set; } = 0;

    /// <summary>
    /// Gets the number of documents to take (limit) in the query result.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    internal int Take { get; private set; } = 100;

    /// <summary>
    /// Gets the sorting expression for the query results.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    internal Expression<Func<TDocument, object>>? OrderExpression { get; private set; }

    /// <summary>
    /// Gets whether sorting should be in descending order.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    internal bool OrderByDesc { get; private set; } = true;

    /// <summary>
    /// Creates a new builder instance with default values.
    /// </summary>
    /// <returns>A new <see cref="RangeQueryDocumentBuilder{TDocument}"/> instance.</returns>
    public static RangeQueryDocumentBuilder<TDocument> Create() => new();

    /// <summary>
    /// Disables builder constraints (like maximum take limit).
    /// This method should be used with caution as it bypasses safety checks.
    /// </summary>
    /// <returns>The current builder instance.</returns>
    [Obsolete("Do not use disabling builder restrictions unless it is done intentionally")]
    [EditorBrowsable(EditorBrowsableState.Never)]
    public RangeQueryDocumentBuilder<TDocument> WithIgnoreBuilderConstraints()
    {
        ignoreBuilderConstraints = true;
        return this;
    }

    /// <summary>
    /// Sets the filter expression for the query.
    /// </summary>
    /// <param name="filter">The filter expression to apply.</param>
    /// <returns>The current builder instance with updated filter.</returns>
    /// <exception cref="InvalidArgumentException">Thrown when filter is null.</exception>
    public RangeQueryDocumentBuilder<TDocument> WithFilter(Expression<Func<TDocument, bool>> filter)
    {
        Filter = filter ?? throw new InvalidArgumentException($"Using a {nameof(WithFilter)} without f expression is not allowed");
        return this;
    }

    /// <summary>
    /// Sets the pagination parameters for the query.
    /// </summary>
    /// <param name="skip">Number of documents to skip.</param>
    /// <param name="take">Number of documents to take.</param>
    /// <returns>The current builder instance with updated pagination settings.</returns>
    /// <exception cref="InvalidArgumentException">
    /// Thrown when skip is negative or take is not positive.
    /// </exception>
    public RangeQueryDocumentBuilder<TDocument> WithPagination(int skip, int take)
    {
        if (skip < 0)
            throw new InvalidArgumentException($"Using a {nameof(WithPagination)} with {nameof(skip)} param less than zero is not allowed");

        if (take <= 0)
            throw new InvalidArgumentException($"Using a {nameof(WithPagination)} with {nameof(take)} param less or zero is not allowed");

        if (take > 1000 && !ignoreBuilderConstraints)
            throw new InvalidArgumentException($"Using a {nameof(WithPagination)} with {nameof(take)} param more than 1000 is not allowed");

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
    /// <exception cref="InvalidArgumentException">Thrown when orderBy is null.</exception>
    public RangeQueryDocumentBuilder<TDocument> WithOrdering(
        Expression<Func<TDocument, object>> orderBy,
        bool descending = true)
    {
        OrderExpression = orderBy ?? throw new InvalidArgumentException($"Using a {nameof(WithOrdering)} without order expression is not allowed");
        OrderByDesc = descending;
        return this;
    }
}

