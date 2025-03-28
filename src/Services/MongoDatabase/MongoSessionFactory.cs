using Infrastructure.Abstractions.Factory;
using MongoDB.Driver;

namespace Infrastructure.Services.MongoDatabase;

/// <summary>
/// Provides factory methods for creating MongoDB sessions with a specific MongoDB context.
/// This class implements both the generic and non-generic session factory interfaces.
/// </summary>
/// <typeparam name="TMongoContext">The type of the MongoDB context used to create sessions.</typeparam>
/// <remarks>
/// The factory delegates session creation to the underlying MongoDB context while providing
/// a consistent interface for session management. Sessions created by this factory should
/// be properly disposed when no longer needed.
/// </remarks>
public class MongoSessionFactory<TMongoContext>(TMongoContext context) :
    IMongoSessionFactory, IMongoSessionFactory<TMongoContext>
    where TMongoContext : MongoDatabaseContext
{
    /// <summary>
    /// Begins a new synchronous MongoDB session.
    /// </summary>
    /// <returns>An <see cref="IClientSession"/> representing the new session.</returns>
    /// <remarks>
    /// This method creates a new client session that can be used for transactions or
    /// other session-dependent operations. The session should be disposed when no
    /// longer needed to free up server resources.
    /// </remarks>
    public IClientSession BeginSession()
        => context.StartSession();

    /// <summary>
    /// Begins a new asynchronous MongoDB session.
    /// </summary>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous operation and returns an <see cref="IClientSession"/> when completed.</returns>
    /// <remarks>
    /// This async method creates a new client session that can be used for transactions or
    /// other session-dependent operations. The session should be disposed when no
    /// longer needed to free up server resources.
    /// </remarks>
    public Task<IClientSession> BeginSessionAsync(CancellationToken cancellationToken = default)
        => context.StartSessionAsync(cancellationToken);
}
