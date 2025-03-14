using Infrastructure.Options;
using Infrastructure.Services.MongoDatabase.Document;
using MongoDB.Driver;

namespace Infrastructure.Services.MongoDatabase;

/// <summary>
/// An abstract base class representing a MongoDB database context.
/// This class provides access to a MongoDB database and manages the lifecycle of the MongoDB client.
/// This class is responsible for initializing the MongoDB client and database, and it implements <see cref="IDisposable"/>
/// to ensure proper cleanup of resources. It also provides a method to retrieve a document set for a specific document type.
/// </remarks>
public abstract class MongoDatabaseContext : IDisposable
{
    private readonly Lazy<MongoClient> _client;
    private readonly Lazy<IMongoDatabase> _database;

    private bool _disposed = false;

    /// <summary>
    /// This constructor lazy initializing the MongoDB client and database
    /// </summary>
    /// <param name="configureOptions">Configuration options for connecting to the MongoDB database.</param>
    protected MongoDatabaseContext(MongoDatabaseConfigureOptions configureOptions)
    {
        _client = new Lazy<MongoClient>(() => new(configureOptions.Connection), LazyThreadSafetyMode.ExecutionAndPublication);
        _database = new Lazy<IMongoDatabase>(() => _client.Value.GetDatabase(configureOptions.Database), LazyThreadSafetyMode.ExecutionAndPublication);
    }

    /// <summary>
    /// Gets or sets the MongoDB database instance.
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

    protected virtual IMongoCollection<TDocument> SetDocument<TDocument>() where TDocument : DocumentBase, new()
    {
        var document = new TDocument();
        return Database.GetCollection<TDocument>(document.CollectionName);
    }

    public virtual IMongoCollection<TDocument> SetDocument<TDocument>(TDocument document) where TDocument : DocumentBase
        => Database.GetCollection<TDocument>(document.CollectionName);

    /// <summary>
    /// Finalizes the <see cref="MongoDatabaseContext"/> instance.
    /// </summary>
    ~MongoDatabaseContext()
    {
        Dispose(false);
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
        if (_disposed)
            return;

        if (disposing && _client.IsValueCreated)
            _client.Value.Dispose();

        _disposed = true;
    }
}
