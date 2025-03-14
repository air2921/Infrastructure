using Infrastructure.Options;
using MongoDB.Driver;

namespace Infrastructure.Services.MongoDatabase;

/// <summary>
/// Provides a managed way to interact with a MongoDB database.
/// This class encapsulates the creation and disposal of a MongoDB client and database instance,
/// ensuring that resources are properly released when no longer needed.
/// </summary>
/// <remarks>
/// This class implements the <see cref="IDisposable"/> interface to ensure that the MongoDB client
/// is properly disposed of, preventing resource leaks.
/// </remarks>
public class MongoDatabaseProvider(MongoDatabaseConfigureOptions configureOptions) : IDisposable
{
    private bool _disposed = false;

    private readonly MongoClient _client = new(configureOptions.Connection);
    private IMongoDatabase _database = default!;

    /// <summary>
    /// Gets or sets the MongoDB database instance.
    /// </summary>
    /// <exception cref="ObjectDisposedException">
    /// Thrown if the <see cref="MongoDatabaseProvider"/> has already been disposed.
    /// </exception>
    /// <example>
    /// <code>
    /// var provider = new MongoDatabaseProvider(configureOptions);
    /// var database = provider.Database; // Gets the database instance.
    /// provider.Database = provider.Database; // Sets the database instance.
    /// </code>
    /// </example>
    public IMongoDatabase Database
    {
        get
        {
            ObjectDisposedException.ThrowIf(_disposed, this);
            return _database;
        }
        set
        {
            ObjectDisposedException.ThrowIf(_disposed, this);
            _database = _client.GetDatabase(configureOptions.Connection);
        }
    }

    /// <summary>
    /// Finalizes an instance of the <see cref="MongoDatabaseProvider"/> class.
    /// This destructor ensures that resources are released if <see cref="Dispose()"/> is not called.
    /// </summary>
    ~MongoDatabaseProvider()
    {
        Dispose(false);
    }

    /// <summary>
    /// Releases all resources used by the <see cref="MongoDatabaseProvider"/>.
    /// This method is part of the <see cref="IDisposable"/> interface and should be called
    /// to explicitly release resources when the provider is no longer needed.
    /// </summary>
    /// <example>
    /// <code>
    /// using (var provider = new MongoDatabaseProvider(configureOptions))
    /// {
    ///     // Use the provider to interact with the database.
    /// }
    /// // The provider is automatically disposed at the end of the using block.
    /// </code>
    /// </example>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Releases the unmanaged resources used by the <see cref="MongoDatabaseProvider"/> and optionally releases the managed resources.
    /// </summary>
    /// <param name="disposing">
    /// <c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.
    /// </param>
    protected virtual void Dispose(bool disposing)
    {
        if (_disposed)
            return;

        if (disposing)
        {
            _client.Dispose();
        }

        _disposed = true;
    }
}
