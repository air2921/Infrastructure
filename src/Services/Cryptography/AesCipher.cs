﻿using Infrastructure.Abstractions.Cryptography;
using Infrastructure.Exceptions;
using Microsoft.Extensions.Logging;
using System.Security.Cryptography;

namespace Infrastructure.Services.Cryptography;

/// <summary>
/// A class responsible for encrypting and decrypting data using AES algorithm with CBC mode and PKCS7 padding.
/// This class implements the <see cref="ICipher"/> interface to provide encryption and decryption functionality.
/// </summary>
/// <param name="logger">A logger for tracking errors and operations performed by this class.</param>
/// <remarks>
/// This class uses AES (Advanced Encryption Standard) algorithm with:
/// - CBC (Cipher Block Chaining) mode
/// - PKCS7 padding
/// - Key derivation using Rfc2898DeriveBytes with SHA256 and 1000 iterations
/// - Supports both synchronous and asynchronous operations
/// - Includes cancellation support for async operations
/// Errors during encryption or decryption are logged using the provided logger.
/// </remarks>
public class AesCipher(ILogger<AesCipher> logger) : ICipher
{
    /// <summary>
    /// Asynchronously encrypts data from the source stream and writes the result to the target stream.
    /// </summary>
    /// <param name="source">The source stream containing data to encrypt.</param>
    /// <param name="target">The target stream where encrypted data will be written.</param>
    /// <param name="key">The encryption key to use.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous encryption operation.</returns>
    /// <exception cref="CryptographyException">Thrown if an error occurs during encryption.</exception>
    /// <remarks>
    /// The encryption process:
    /// 1. Generates a random IV (Initialization Vector)
    /// 2. Writes the IV to the beginning of the target stream
    /// 3. Derives a key using PBKDF2 (Rfc2898DeriveBytes)
    /// 4. Encrypts the data using AES-CBC with the derived key
    /// The operation can be cancelled using the provided cancellation token.
    /// </remarks>
    public Task EncryptAsync(Stream source, Stream target, byte[] key, CancellationToken cancellationToken = default)
    {
        try
        {
            using var aes = Aes.Create();
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;

            byte[] iv = aes.IV;
            target.Write(iv);

            using var rfc2898 = new Rfc2898DeriveBytes(key, iv, 1000, HashAlgorithmName.SHA256);
            aes.Key = rfc2898.GetBytes(aes.KeySize / 8);

            using var cryptoStream = new CryptoStream(target, aes.CreateEncryptor(), CryptoStreamMode.Write);
            return source.CopyToAsync(cryptoStream, cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while attempting to decrypt data", [key.Length]);
            throw new CryptographyException("An error occurred while attempting to decrypt data");
        }
    }

    /// <summary>
    /// Encrypts data from the source stream and writes the result to the target stream.
    /// </summary>
    /// <param name="source">The source stream containing data to encrypt.</param>
    /// <param name="target">The target stream where encrypted data will be written.</param>
    /// <param name="key">The encryption key to use.</param>
    /// <exception cref="CryptographyException">Thrown if an error occurs during encryption.</exception>
    /// <remarks>
    /// The encryption process:
    /// 1. Generates a random IV (Initialization Vector)
    /// 2. Writes the IV to the beginning of the target stream
    /// 3. Derives a key using PBKDF2 (Rfc2898DeriveBytes)
    /// 4. Encrypts the data using AES-CBC with the derived key
    /// Note: This is the synchronous version of the operation.
    /// Warning: For large streams, consider using the async version instead.
    /// </remarks>
    public void Encrypt(Stream source, Stream target, byte[] key)
    {
        try
        {
            using var aes = Aes.Create();
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;

            byte[] iv = aes.IV;
            target.Write(iv);

            using var rfc2898 = new Rfc2898DeriveBytes(key, iv, 1000, HashAlgorithmName.SHA256);
            aes.Key = rfc2898.GetBytes(aes.KeySize / 8);

            using var cryptoStream = new CryptoStream(target, aes.CreateEncryptor(), CryptoStreamMode.Write);
            source.CopyToAsync(cryptoStream);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while attempting to decrypt data", [key.Length]);
            throw new CryptographyException("An error occurred while attempting to decrypt data");
        }
    }

    /// <summary>
    /// Asynchronously decrypts data from the source stream and writes the result to the target stream.
    /// </summary>
    /// <param name="source">The source stream containing encrypted data.</param>
    /// <param name="target">The target stream where decrypted data will be written.</param>
    /// <param name="key">The decryption key to use.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous decryption operation.</returns>
    /// <exception cref="CryptographyException">Thrown if an error occurs during decryption.</exception>
    /// <remarks>
    /// The decryption process:
    /// 1. Reads the IV from the beginning of the source stream
    /// 2. Derives a key using PBKDF2 (Rfc2898DeriveBytes)
    /// 3. Decrypts the data using AES-CBC with the derived key
    /// The operation can be cancelled using the provided cancellation token.
    /// </remarks>
    public Task DecryptAsync(Stream source, Stream target, byte[] key, CancellationToken cancellationToken = default)
    {
        try
        {
            using var aes = Aes.Create();
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;

            byte[] iv = new byte[aes.BlockSize / 8];
            source.Read(iv);
            aes.IV = iv;

            using var rfc2898 = new Rfc2898DeriveBytes(key, iv, 1000, HashAlgorithmName.SHA256);
            aes.Key = rfc2898.GetBytes(aes.KeySize / 8);

            using var cryptoStream = new CryptoStream(source, aes.CreateDecryptor(), CryptoStreamMode.Read);
            return cryptoStream.CopyToAsync(target, cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while attempting to encrypt data", [key.Length]);
            throw new CryptographyException("An error occurred while attempting to encrypt data");
        }
    }

    /// <summary>
    /// Decrypts data from the source stream and writes the result to the target stream.
    /// </summary>
    /// <param name="source">The source stream containing encrypted data.</param>
    /// <param name="target">The target stream where decrypted data will be written.</param>
    /// <param name="key">The decryption key to use.</param>
    /// <exception cref="CryptographyException">Thrown if an error occurs during decryption.</exception>
    /// <remarks>
    /// The decryption process:
    /// 1. Reads the IV from the beginning of the source stream
    /// 2. Derives a key using PBKDF2 (Rfc2898DeriveBytes)
    /// 3. Decrypts the data using AES-CBC with the derived key
    /// Note: This is the synchronous version of the operation.
    /// Warning: For large streams, consider using the async version instead.
    /// </remarks>
    public void Decrypt(Stream source, Stream target, byte[] key)
    {
        try
        {
            using var aes = Aes.Create();
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;

            byte[] iv = new byte[aes.BlockSize / 8];
            source.Read(iv);
            aes.IV = iv;

            using var rfc2898 = new Rfc2898DeriveBytes(key, iv, 1000, HashAlgorithmName.SHA256);
            aes.Key = rfc2898.GetBytes(aes.KeySize / 8);

            using var cryptoStream = new CryptoStream(source, aes.CreateDecryptor(), CryptoStreamMode.Read);
            cryptoStream.CopyToAsync(target);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while attempting to encrypt data", [key.Length]);
            throw new CryptographyException("An error occurred while attempting to encrypt data");
        }
    }
}
