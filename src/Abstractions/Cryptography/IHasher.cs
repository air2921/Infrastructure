using BCrypt.Net;
using Infrastructure.Exceptions;

namespace Infrastructure.Abstractions.Cryptography;

/// <summary>
/// Interface for hashing passwords and verifying hashed strings.
/// </summary>
public interface IHasher
{
    /// <summary>
    /// Hashes a password using the specified hash algorithm.
    /// </summary>
    /// <param name="password">The password to hash.</param>
    /// <param name="hashType">The hash type to use for enhanced hashing. Defaults to <see cref="HashType.SHA512"/>.</param>
    /// <returns>A hashed string representation of the password.</returns>
    /// <exception cref="CryptographyException">Thrown when an error occurs during the hashing process (e.g., invalid algorithm or other cryptographic issues).</exception>
    public string Hash(string password, HashType hashType = HashType.SHA512);

    /// <summary>
    /// Verifies if an input string matches a previously hashed string.
    /// </summary>
    /// <param name="input">The input string to verify (e.g., a password).</param>
    /// <param name="src">The previously hashed string to compare against.</param>
    /// <param name="hashType">The hash type used for enhanced verification. Defaults to <see cref="HashType.SHA512"/>.</param>
    /// <returns><c>true</c> if the input matches the hashed string; otherwise, <c>false</c>.</returns>
    /// <exception cref="CryptographyException">Thrown when an error occurs during the verification process (e.g., invalid algorithm or cryptographic failure).</exception>
    public bool Verify(string input, string src, HashType hashType = HashType.SHA512);
}