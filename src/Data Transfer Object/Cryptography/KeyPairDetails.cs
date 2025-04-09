namespace Infrastructure.Data_Transfer_Object.Cryptography;

/// <summary>
/// Represents a cryptographic key pair containing both public and private keys.
/// This class is typically used to store asymmetric encryption keys such as RSA or ECDSA key pairs.
/// </summary>
/// <remarks>
/// The keys are stored as byte arrays to maintain flexibility with different cryptographic algorithms.
/// Both keys are required and should be kept secure, especially the private key which should be protected from unauthorized access.
/// </remarks>
public class KeyPairDetails
{
    /// <summary>
    /// Gets the public key component of the key pair.
    /// The public key can be freely distributed and is typically used for encryption or signature verification.
    /// </summary>
    public required byte[] PublicKey { get; init; }

    /// <summary>
    /// Gets the private key component of the key pair.
    /// The private key must be kept confidential and is used for decryption or creating digital signatures.
    /// </summary>
    public required byte[] PrivateKey { get; init; }
}
