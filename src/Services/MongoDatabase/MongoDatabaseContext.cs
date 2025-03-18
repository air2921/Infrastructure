using Infrastructure.Exceptions;
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
    private IClientSessionHandle? _session;
    private readonly SemaphoreSlim _sessionSemaphore = new(1, 1);
    private bool _disposed = false;

    private static readonly Lazy<EntityException> _operationCancelledError = new(() => new("Operation was cancelled"), LazyThreadSafetyMode.ExecutionAndPublication);
    private static readonly Lazy<EntityException> _sessionAlreadyCreated = new(() => new("Session already created"), LazyThreadSafetyMode.ExecutionAndPublication);
    private static readonly Lazy<EntityException> _transactionNotStarted = new(() => new("Transaction is not started"), LazyThreadSafetyMode.ExecutionAndPublication);

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
    /// Finalizes the <see cref="MongoDatabaseContext"/> instance.
    /// </summary>
    ~MongoDatabaseContext()
    {
        Dispose(false);
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
    /// Starts a new transaction.
    /// </summary>
    /// <exception cref="EntityException">Thrown if a transaction is already in progress.</exception>
    public virtual void BeginTransaction()
    {
        _sessionSemaphore.Wait();
        try
        {
            if (_session is not null)
                throw _sessionAlreadyCreated.Value;

            _session = _client.Value.StartSession();
            _session.StartTransaction();
        }
        finally
        {
            _sessionSemaphore.Release();
        }
    }

    /// <summary>
    /// Commits the current transaction.
    /// </summary>
    /// <exception cref="EntityException">Thrown if no transaction is in progress.</exception>
    public virtual void CommitTransaction()
    {
        _sessionSemaphore.Wait();
        try
        {
            if (_session is null || !_session.IsInTransaction)
                throw _transactionNotStarted.Value;

            _session.CommitTransaction();
            _session.Dispose();
            _session = null;
        }
        finally
        {
            _sessionSemaphore.Release();
        }
    }

    /// <summary>
    /// Rolls back the current transaction.
    /// </summary>
    /// <exception cref="EntityException">Thrown if no transaction is in progress.</exception>
    public virtual void RollbackTransaction()
    {
        _sessionSemaphore.Wait();
        try
        {
            if (_session is null || !_session.IsInTransaction)
                throw _transactionNotStarted.Value;

            _session.AbortTransaction();
            _session.Dispose();
            _session = null;
        }
        finally
        {
            _sessionSemaphore.Release();
        }
    }

    /// <summary>
    /// Starts a new transaction asynchronously. This method ensures thread-safe access to the session
    /// by using a semaphore to prevent concurrent access. If a session is already created, an
    /// <see cref="EntityException"/> is thrown.
    /// </summary>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> that can be used to cancel the operation.</param>
    /// <exception cref="EntityException">Thrown if a session is already created or a transaction is already in progress.</exception>
    public virtual async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        await _sessionSemaphore.WaitAsync(cancellationToken);
        try
        {
            if (_session is not null)
                throw _sessionAlreadyCreated.Value;

            _session = await _client.Value.StartSessionAsync(new ClientSessionOptions(), cancellationToken);
            _session.StartTransaction();
        }
        catch (OperationCanceledException)
        {
            throw _operationCancelledError.Value;
        }
        finally
        {
            _sessionSemaphore.Release();
        }
    }

    /// <summary>
    /// Commits the current transaction asynchronously. This method ensures thread-safe access to the session
    /// by using a semaphore to prevent concurrent access. If no transaction is in progress, an
    /// <see cref="EntityException"/> is thrown.
    /// </summary>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> that can be used to cancel the operation.</param>
    /// <exception cref="EntityException">Thrown if no transaction is in progress.</exception>
    public virtual async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
    {
        await _sessionSemaphore.WaitAsync(cancellationToken);
        try
        {
            if (_session is null || !_session.IsInTransaction)
                throw _transactionNotStarted.Value;

            await _session.CommitTransactionAsync(cancellationToken);
            _session.Dispose();
            _session = null;
        }
        catch (OperationCanceledException)
        {
            throw _operationCancelledError.Value;
        }
        finally
        {
            _sessionSemaphore.Release();
        }
    }

    /// <summary>
    /// Rolls back the current transaction asynchronously. This method ensures thread-safe access to the session
    /// by using a semaphore to prevent concurrent access. If no transaction is in progress, an
    /// <see cref="EntityException"/> is thrown.
    /// </summary>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> that can be used to cancel the operation.</param>
    /// <exception cref="EntityException">Thrown if no transaction is in progress.</exception>
    public virtual async Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
    {
        await _sessionSemaphore.WaitAsync(cancellationToken);
        try
        {
            if (_session is null || !_session.IsInTransaction)
                throw _transactionNotStarted.Value;

            await _session.AbortTransactionAsync(cancellationToken);
            _session.Dispose();
            _session = null;
        }
        catch (OperationCanceledException)
        {
            throw _operationCancelledError.Value;
        }
        finally
        {
            _sessionSemaphore.Release();
        }
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
        {
            if (_client.IsValueCreated)
                _client.Value.Dispose();

            _sessionSemaphore.Wait();
            try
            {
                if (_session is not null)
                {
                    _session.Dispose();
                    _session = null;
                }
            }
            finally
            {
                _sessionSemaphore.Release();
            }

            _sessionSemaphore.Dispose();
        }

        _disposed = true;
    }
}
