using Infrastructure.Abstractions.Cryptography;

namespace Infrastructure.Services.Cryptography.Oqs.Format;

/// <summary>
/// Provides format specifications for ML-DSA-44 (Module Lattice Digital Signature Algorithm 44)
/// </summary>
/// <remarks>
/// Implements parameters for the 44-bit security variant of ML-DSA according to NIST PQC standards.
/// </remarks>
public readonly struct MLDSA44Format : IOqsAlgorithmFormat
{
    /// <summary>
    /// The standardized algorithm name string "ML-DSA-44"
    /// </summary>
    /// <remarks>
    /// Corresponds to FIPS 204 ML-DSA specification with 44-bit security level.
    /// Represents Module-Lattice based Digital Signature Algorithm.
    /// </remarks>
    private const string AlgorithmIdentifier = "ML-DSA-44";

    /// <summary>
    /// Gets the algorithm name identifier
    /// </summary>
    /// <value>
    /// Always returns "ML-DSA-44" string constant
    /// </value>
    public string Algorithm => AlgorithmIdentifier;

    /// <summary>
    /// Gets the signature length of 2420 bytes for ML-DSA-44
    /// </summary>
    public int SignatureLength => 2420;

    /// <summary>
    /// Gets the public key length of 1312 bytes for ML-DSA-44
    /// </summary>
    public int PublicKeyLength => 1312;

    /// <summary>
    /// Gets the private key length of 2560 bytes for ML-DSA-44
    /// </summary>
    public int PrivateKeyLength => 2560;
}
