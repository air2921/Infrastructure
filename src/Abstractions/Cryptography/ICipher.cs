using Infrastructure.Exceptions;

namespace Infrastructure.Abstractions.Cryptography;

/// <summary>
/// Interface for encrypting and decrypting data streams.
/// </summary>
public interface ICipher
{
    /// <summary>
    /// Asynchronously encrypts data from source stream to target stream using the specified key.
    /// </summary>
    /// <param name="source">Stream containing data to encrypt.</param>
    /// <param name="target">Stream to write encrypted data to.</param>
    /// <param name="key">Encryption key to use.</param>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    /// <exception cref="CryptographyException">Thrown when encryption fails.</exception>
    public Task EncryptAsync(Stream source, Stream target, byte[] key, CancellationToken cancellationToken = default);

    /// <summary>
    /// Synchronously encrypts data from source stream to target stream using the specified key.
    /// </summary>
    /// <param name="source">Stream containing data to encrypt.</param>
    /// <param name="target">Stream to write encrypted data to.</param>
    /// <param name="key">Encryption key to use.</param>
    /// <exception cref="CryptographyException">Thrown when encryption fails.</exception>
    public void Encrypt(Stream source, Stream target, byte[] key);

    /// <summary>
    /// Asynchronously decrypts data from source stream to target stream using the specified key.
    /// </summary>
    /// <param name="source">Stream containing encrypted data.</param>
    /// <param name="target">Stream to write decrypted data to.</param>
    /// <param name="key">Decryption key to use.</param>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    /// <exception cref="CryptographyException">Thrown when decryption fails.</exception>
    public Task DecryptAsync(Stream source, Stream target, byte[] key, CancellationToken cancellationToken = default);

    /// <summary>
    /// Synchronously decrypts data from source stream to target stream using the specified key.
    /// </summary>
    /// <param name="source">Stream containing encrypted data.</param>
    /// <param name="target">Stream to write decrypted data to.</param>
    /// <param name="key">Decryption key to use.</param>
    /// <exception cref="CryptographyException">Thrown when decryption fails.</exception>
    public void Decrypt(Stream source, Stream target, byte[] key);
}
