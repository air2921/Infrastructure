using Infrastructure.Data_Transfer_Object.Base;

namespace Infrastructure.Abstractions;

/// <summary>
/// Interface for message senders.
/// </summary>
/// <typeparam name="TMessage">The type of message, derived from <see cref="MessageDetails"/>.</typeparam>
public interface ISender<TMessage> where TMessage : MessageDetails
{
    /// <summary>
    /// Asynchronously sends a message.
    /// </summary>
    /// <param name="message">The message to be sent.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public Task SendAsync(TMessage message, CancellationToken cancellationToken = default);

    /// <summary>
    /// Synchronously sends a message.
    /// </summary>
    /// <param name="message">The message to be sent.</param>
    public void Send(TMessage message);
}
