using Infrastructure.Abstractions.Cryptography;

namespace Infrastructure.Services.Cryptography.Oqs.Format;

/// <summary>
/// Provides format specifications for Falcon-512 (Fast-Fourier Lattice-based Compact Signatures)
/// </summary>
/// <remarks>
/// Implements parameters for the 512-bit security variant of Falcon according to NIST PQC standards.
/// </remarks>
public readonly struct Falcon512Format : IOqsAlgorithmFormat
{
    /// <summary>
    /// The standardized algorithm name string "Falcon-512"
    /// </summary>
    /// <remarks>
    /// Corresponds to FIPS 205 SLH-DSA specification with 512-bit security level
    /// </remarks>
    private const string AlgorithmIdentifier = "Falcon-512";

    /// <summary>
    /// Gets the algorithm name identifier
    /// </summary>
    /// <value>
    /// Always returns "Falcon-512" string constant
    /// </value>
    public string Algorithm => AlgorithmIdentifier;

    /// <summary>
    /// Gets the signature length of 666 bytes for Falcon-512
    /// </summary>
    /// <value>Signature size in bytes</value>
    public int SignatureLength => 666;

    /// <summary>
    /// Gets the public key length of 897 bytes for Falcon-512
    /// </summary>
    /// <value>Public key size in bytes</value>
    public int PublicKeyLength => 897;

    /// <summary>
    /// Gets the private key length of 1281 bytes for Falcon-512
    /// </summary>
    /// <value>Private key size in bytes</value>
    public int PrivateKeyLength => 1281;
}
