using Infrastructure.Exceptions;
using Infrastructure.Options;
using Infrastructure.Services.MongoDatabase.Document;
using MongoDB.Driver;

namespace Infrastructure.Services.MongoDatabase;

/// <summary>
/// An abstract base class representing a MongoDB database context.
/// This class provides access to a MongoDB database and manages the lifecycle of the MongoDB client.
/// Implements <see cref="IDisposable"/> to ensure proper cleanup of resources.
/// </summary>
/// <remarks>
/// The context manages MongoDB client and database instances using lazy initialization,
/// and provides transactional support through session management when available.
/// Transactions require MongoDB replica set or sharded cluster (v4.0+ for replica sets, v4.2+ for sharded clusters).
/// </remarks>
public abstract class MongoContext : IDisposable
{
    private readonly MongoClient _client;
    private readonly Lazy<IMongoDatabase> _database;
    private volatile bool _disposed = false;

    /// <summary>
    /// Initializes a new instance of the <see cref="MongoContext"/> class.
    /// </summary>
    /// <param name="configureOptions">Configuration options for connecting to MongoDB.</param>
    /// <exception cref="EntityException">Thrown when transactions are enabled but not supported by the MongoDB deployment.</exception>
    protected MongoContext(MongoDatabaseConfigureOptions configureOptions)
    {
        _client = new MongoClient(configureOptions.Connection);
        _database = new Lazy<IMongoDatabase>(() => _client.GetDatabase(configureOptions.Database));
    }

    /// <summary>
    /// Gets the MongoDB database instance.
    /// </summary>
    /// <exception cref="ObjectDisposedException">Thrown if the context has been disposed.</exception>
    public IMongoDatabase Database
    {
        get
        {
            ObjectDisposedException.ThrowIf(_disposed, this);
            return _database.Value;
        }
    }

    /// <summary>
    /// Gets a MongoDB collection for the specified document type.
    /// </summary>
    /// <typeparam name="TDocument">The document type, must inherit from <see cref="DocumentBase"/>.</typeparam>
    /// <returns>An <see cref="IMongoCollection{TDocument}"/> instance.</returns>
    protected virtual IMongoCollection<TDocument> SetDocument<TDocument>() where TDocument : DocumentBase, new()
    {
        var document = new TDocument();
        return Database.GetCollection<TDocument>(document.CollectionName);
    }

    /// <summary>
    /// Gets a MongoDB collection for the specified document instance.
    /// </summary>
    /// <typeparam name="TDocument">The document type, must inherit from <see cref="DocumentBase"/>.</typeparam>
    /// <param name="document">The document instance containing the collection name.</param>
    /// <returns>An <see cref="IMongoCollection{TDocument}"/> instance.</returns>
    public virtual IMongoCollection<TDocument> SetDocument<TDocument>(TDocument document) where TDocument : DocumentBase
        => Database.GetCollection<TDocument>(document.CollectionName);

    /// <summary>
    /// Starts a new client session.
    /// </summary>
    /// <returns>A <see cref="IClientSessionHandle"/> representing the session.</returns>
    /// <exception cref="ObjectDisposedException">Thrown if the context has been disposed.</exception>
    public virtual IClientSessionHandle StartSession()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        return _client.StartSession();
    }

    /// <summary>
    /// Asynchronously starts a new client session.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    /// <exception cref="ObjectDisposedException">Thrown if the context has been disposed.</exception>
    public virtual async Task<IClientSessionHandle> StartSessionAsync(CancellationToken cancellationToken = default)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        return await _client.StartSessionAsync(cancellationToken: cancellationToken);
    }

    /// <summary>
    /// Disposes the context and releases resources.
    /// </summary>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Protected implementation of Dispose pattern.
    /// </summary>
    /// <param name="disposing">True to release both managed and unmanaged resources.</param>
    protected virtual void Dispose(bool disposing)
    {
        if (_disposed)
            return;

        if (disposing)
        {
            _client?.Dispose();
        }

        _disposed = true;
    }
}
