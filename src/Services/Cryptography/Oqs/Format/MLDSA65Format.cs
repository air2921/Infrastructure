using Infrastructure.Abstractions.Cryptography;

namespace Infrastructure.Services.Cryptography.Oqs.Format;

/// <summary>
/// Provides format specifications for ML-DSA-65 (Module Lattice Digital Signature Algorithm 65) 
/// </summary>
/// <remarks>
/// Implements parameters for the 65-bit security variant of ML-DSA according to NIST PQC standards.
/// </remarks>
public readonly struct MLDSA65Format : IOqsAlgorithmFormat
{
    /// <summary>
    /// The standardized algorithm name string "ML-DSA-65"
    /// </summary>
    /// <remarks>
    /// Corresponds to FIPS 204 ML-DSA specification with 65-bit security level.
    /// Represents Module-Lattice based Digital Signature Algorithm.
    /// </remarks>
    private const string AlgorithmIdentifier = "ML-DSA-65";

    /// <summary>
    /// Gets the algorithm name identifier
    /// </summary>
    /// <value>
    /// Always returns "ML-DSA-65" string constant
    /// </value>
    public string Algorithm => AlgorithmIdentifier;

    /// <summary>
    /// Gets the signature length of 3309 bytes for ML-DSA-65
    /// </summary>
    public int SignatureLength => 3309;

    /// <summary>
    /// Gets the public key length of 1952 bytes for ML-DSA-65
    /// </summary>
    public int PublicKeyLength => 1952;

    /// <summary>
    /// Gets the private key length of 4032 bytes for ML-DSA-65
    /// </summary>
    public int PrivateKeyLength => 4032;
}
