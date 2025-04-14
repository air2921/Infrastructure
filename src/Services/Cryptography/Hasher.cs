using BCrypt.Net;
using Infrastructure.Abstractions.Cryptography;
using Infrastructure.Exceptions;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Services.Cryptography;

/// <summary>
/// A class responsible for hashing and verifying strings using the BCrypt algorithm.
/// This class implements the <see cref="IHasher"/> interface to provide hashing and verification functionality.
/// </summary>
/// <param name="logger">A logger for tracking errors and operations performed by this class.</param>
/// <remarks>
/// This class uses the BCrypt algorithm to hash and verify strings. It supports enhanced hashing with different hash types,
/// such as SHA512. Errors during hashing or verification are logged using the provided logger.
/// </remarks>
public class Hasher(ILogger<Hasher> logger) : IHasher
{
    /// <summary>
    /// Hashes a password using the BCrypt algorithm.
    /// </summary>
    /// <param name="password">The password to hash.</param>
    /// <param name="hashType">The hash type to use for enhanced hashing. Defaults to <see cref="HashType.SHA512"/>.</param>
    /// <returns>A hashed string representation of the password.</returns>
    /// <exception cref="CryptographyException">Thrown if an error occurs during hashing.</exception>
    public string Hash(string password, HashType hashType = HashType.SHA512)
    {
        try
        {
            return BCrypt.Net.BCrypt.EnhancedHashPassword(password, hashType);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while hashing the object, via {hashtype}", hashType);
            throw new CryptographyException("An error occurred while hashing the object");
        }
    }

    /// <summary>
    /// Verifies if an input string matches a previously hashed string.
    /// </summary>
    /// <param name="input">The input string to verify (e.g., a password).</param>
    /// <param name="src">The previously hashed string to compare against.</param>
    /// <param name="hashType">The hash type used for enhanced verification. Defaults to <see cref="HashType.SHA512"/>.</param>
    /// <returns><c>true</c> if the input matches the hashed string; otherwise, <c>false</c>.</returns>
    /// <exception cref="CryptographyException">Thrown if an error occurs during verification.</exception>
    public bool Verify(string input, string src, HashType hashType = HashType.SHA512)
    {
        try
        {
            return BCrypt.Net.BCrypt.EnhancedVerify(input, src, hashType);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while checking the hash via {hashtype}", hashType);
            throw new CryptographyException("An error occurred while checking the hash");
        }
    }
}
