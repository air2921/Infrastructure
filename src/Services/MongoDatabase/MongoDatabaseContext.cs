using Infrastructure.Options;
using Infrastructure.Services.MongoDatabase.Document;
using MongoDB.Driver;

namespace Infrastructure.Services.MongoDatabase;

/// <summary>
/// An abstract base class representing a MongoDB database context.
/// This class provides access to a MongoDB database and manages the lifecycle of the MongoDB client.
/// </summary>
/// <param name="configureOptions">Configuration options for connecting to the MongoDB database.</param>
/// <remarks>
/// This class is responsible for initializing the MongoDB client and database, and it implements <see cref="IDisposable"/>
/// to ensure proper cleanup of resources. It also provides a method to retrieve a document set for a specific document type.
/// </remarks>
public abstract class MongoDatabaseContext(MongoDatabaseConfigureOptions configureOptions) : IDisposable
{
    public virtual IMongoCollection<TDocument> SetDocument<TDocument>() where TDocument : DocumentBase, new()
    {
        var document = new TDocument();
        return Database.GetCollection<TDocument>(document.CollectionName);
    }

    public virtual IMongoCollection<TDocument> SetDocument<TDocument>(TDocument document) where TDocument : DocumentBase
        => Database.GetCollection<TDocument>(document.CollectionName);

    private bool _disposed = false;

    private readonly MongoClient _client = new(configureOptions.Connection);
    private IMongoDatabase _database = default!;

    /// <summary>
    /// Gets or sets the MongoDB database instance.
    /// </summary>
    /// <exception cref="ObjectDisposedException">Thrown if the context has been disposed.</exception>
    public IMongoDatabase Database
    {
        get
        {
            ObjectDisposedException.ThrowIf(_disposed, this);
            _database = _client.GetDatabase(configureOptions.Connection);
            return _database;
        }
    }

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

        if (disposing)
            _client.Dispose();

        _disposed = true;
    }
}
