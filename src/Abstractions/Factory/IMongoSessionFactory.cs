﻿using Infrastructure.Services.MongoDatabase;
using MongoDB.Driver;

namespace Infrastructure.Abstractions.Factory;

/// <summary>
/// Provides a factory interface for creating MongoDB client sessions.
/// </summary>
/// <remarks>
/// This interface defines the basic operations for starting both synchronous and asynchronous
/// sessions with a MongoDB database. Implementations should ensure proper session lifecycle management.
/// </remarks>
public interface IMongoSessionFactory
{
    /// <summary>
    /// Begins a new synchronous MongoDB client session.
    /// </summary>
    /// <returns>An <see cref="IClientSessionHandle"/> representing the new session.</returns>
    /// <remarks>
    /// <para>
    /// The created session can be used for transactional operations or to ensure causal consistency.
    /// </para>
    /// <para>
    /// The caller is responsible for disposing the session when it's no longer needed to free up server resources.
    /// </para>
    /// </remarks>
    public IClientSessionHandle BeginSession();

    /// <summary>
    /// Begins a new asynchronous MongoDB client session.
    /// </summary>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> that can be used to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the <see cref="IClientSessionHandle"/>.</returns>
    /// <remarks>
    /// <para>
    /// The created session can be used for transactional operations or to ensure causal consistency.
    /// </para>
    /// <para>
    /// The caller is responsible for disposing the session when it's no longer needed to free up server resources.
    /// </para>
    /// </remarks>
    public Task<IClientSessionHandle> BeginSessionAsync(CancellationToken cancellationToken = default);
}

/// <summary>
/// Provides a generic factory interface for creating MongoDB client sessions with a specific context type.
/// </summary>
/// <typeparam name="TMongoContext">The type of MongoDB context used to create sessions, must inherit from <see cref="MongoContext"/>.</typeparam>
/// <remarks>
/// This interface extends <see cref="IMongoSessionFactory"/> without adding new members, serving as a typed marker
/// interface for dependency injection scenarios where a specific context type is required.
/// </remarks>
public interface IMongoSessionFactory<TMongoContext> : IMongoSessionFactory where TMongoContext : MongoContext
{

}
