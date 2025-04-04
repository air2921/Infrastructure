using Infrastructure.Options;
using Infrastructure.Services.MongoDatabase.Document;
using MongoDB.Driver;

namespace Infrastructure.Services.MongoDatabase;

/// <summary>
/// An abstract base class representing a MongoDB database context.
/// This class provides access to a MongoDB database and manages the lifecycle of the MongoDB client.
/// This class is responsible for initializing the MongoDB client and database, and it implements <see cref="IDisposable"/>
/// to ensure proper cleanup of resources. It also provides methods to retrieve document collections and manage database sessions.
/// </summary>
/// <remarks>
/// The context manages MongoDB client and database instances using lazy initialization,
/// and provides transactional support through session management.
/// </remarks>
public abstract class MongoDatabaseContext : IDisposable
{
    private readonly Lazy<MongoClient> _client;
    private readonly Lazy<IMongoDatabase> _database;
    private volatile bool disposed = false;

    /// <summary>
    /// Initializes a new instance of the <see cref="MongoDatabaseContext"/> class with the specified configuration options.
    /// The MongoDB client and database are initialized lazily when first accessed.
    /// </summary>
    /// <param name="configureOptions">Configuration options for connecting to the MongoDB database.</param>
    protected MongoDatabaseContext(MongoDatabaseConfigureOptions configureOptions)
    {
        _client = new Lazy<MongoClient>(() => new(configureOptions.Connection), LazyThreadSafetyMode.ExecutionAndPublication);
        _database = new Lazy<IMongoDatabase>(() => _client.Value.GetDatabase(configureOptions.Database), LazyThreadSafetyMode.ExecutionAndPublication);
    }

    /// <summary>
    /// Gets the MongoDB database instance.
    /// </summary>
    /// <exception cref="ObjectDisposedException">Thrown if the context has been disposed.</exception>
    public IMongoDatabase Database
    {
        get
        {
            ObjectDisposedException.ThrowIf(disposed, this);
            return _database.Value;
        }
    }

    /// <summary>
    /// Gets a MongoDB collection for a specific document type. This method creates a new instance of the document
    /// to retrieve the collection name from the <see cref="DocumentBase.CollectionName"/> property.
    /// </summary>
    /// <typeparam name="TDocument">The type of the document, which must inherit from <see cref="DocumentBase"/> and have a parameterless constructor.</typeparam>
    /// <returns>An <see cref="IMongoCollection{TDocument}"/> instance representing the collection for the specified document type.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the document type does not have a parameterless constructor.</exception>
    protected virtual IMongoCollection<TDocument> SetDocument<TDocument>() where TDocument : DocumentBase, new()
    {
        var document = new TDocument();
        return Database.GetCollection<TDocument>(document.CollectionName);
    }

    /// <summary>
    /// Gets a MongoDB collection for a specific document type using the provided document instance.
    /// The collection name is retrieved from the <see cref="DocumentBase.CollectionName"/> property of the document.
    /// </summary>
    /// <typeparam name="TDocument">The type of the document, which must inherit from <see cref="DocumentBase"/>.</typeparam>
    /// <param name="document">An instance of the document from which to retrieve the collection name.</param>
    /// <returns>An <see cref="IMongoCollection{TDocument}"/> instance representing the collection for the specified document type.</returns>
    public virtual IMongoCollection<TDocument> SetDocument<TDocument>(TDocument document) where TDocument : DocumentBase
        => Database.GetCollection<TDocument>(document.CollectionName);

    /// <summary>
    /// Starts a new client session with the default options for this MongoDB context.
    /// </summary>
    /// <returns>A <see cref="IClientSessionHandle"/> representing the new session.</returns>
    /// <exception cref="ObjectDisposedException">Thrown if the context has been disposed.</exception>
    /// <remarks>
    /// This synchronous method is typically used for transactions and other session-dependent operations.
    /// The session should be disposed when no longer needed to free up resources.
    /// </remarks>
    public virtual IClientSessionHandle StartSession()
    {
        ObjectDisposedException.ThrowIf(disposed, this);
        return _client.Value.StartSession(null, default);
    }

    /// <summary>
    /// Asynchronously starts a new client session with the default options for this MongoDB context.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation and returns a <see cref="IClientSessionHandle"/> when completed.</returns>
    /// <exception cref="ObjectDisposedException">Thrown if the context has been disposed.</exception>
    /// <remarks>
    /// This asynchronous method is typically used for transactions and other session-dependent operations.
    /// The session should be disposed when no longer needed to free up resources.
    /// </remarks>
    public virtual async Task<IClientSessionHandle> StartSessionAsync(CancellationToken cancellationToken = default)
    {
        ObjectDisposedException.ThrowIf(disposed, this);
        return await _client.Value.StartSessionAsync(null, cancellationToken);
    }

    /// <summary>
    /// Releases all resources used by the <see cref="MongoDatabaseContext"/>.
    /// </summary>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Releases the unmanaged resources used by the <see cref="MongoDatabaseContext"/> and optionally releases the managed resources.
    /// </summary>
    /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
    protected virtual void Dispose(bool disposing)
    {
        if (disposed)
            return;

        if (disposing)
        {
            if (_client.IsValueCreated)
                _client.Value.Dispose();
        }

        disposed = true;
    }
}
