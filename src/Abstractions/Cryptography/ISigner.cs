using Infrastructure.Exceptions;

namespace Infrastructure.Abstractions.Cryptography;

/// <summary>
/// Defines methods for generating public/private key pairs, signing messages, and verifying signatures.
/// </summary>
public interface ISigner
{
    /// <summary>
    /// Generates a public/private key pair.
    /// </summary>
    /// <returns>A tuple containing the public and private keys as byte arrays.</returns>
    /// <exception cref="CryptographyException">Thrown if an error occurs while generating the key pair.</exception>
    public (byte[] publicKey, byte[] privateKey) GenerateKeyPair();

    /// <summary>
    /// Signs a message with the provided private key.
    /// </summary>
    /// <param name="message">The message to sign, represented as a byte array.</param>
    /// <param name="privateKey">The private key to sign the message with.</param>
    /// <returns>A byte array containing the signature of the message.</returns>
    /// <exception cref="CryptographyException">Thrown if an error occurs while signing the message.</exception>
    public byte[] Sign(byte[] message, byte[] privateKey);

    /// <summary>
    /// Verifies the authenticity of a signed message.
    /// </summary>
    /// <param name="message">The original message, represented as a byte array.</param>
    /// <param name="signature">The signature of the message to verify, represented as a byte array.</param>
    /// <param name="publicKey">The public key used to verify the signature.</param>
    /// <returns><c>true</c> if the signature is valid, otherwise <c>false</c>.</returns>
    /// <exception cref="CryptographyException">Thrown if an error occurs while verifying the signature.</exception>
    public bool Verify(byte[] message, byte[] signature, byte[] publicKey);
}
