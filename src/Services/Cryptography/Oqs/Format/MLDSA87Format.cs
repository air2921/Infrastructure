using Infrastructure.Abstractions.Cryptography;

namespace Infrastructure.Services.Cryptography.Oqs.Format;

/// <summary>
/// Provides format specifications for ML-DSA-87 (Module Lattice Digital Signature Algorithm 87)
/// </summary>
/// <remarks>
/// Implements parameters for the 87-bit security variant of ML-DSA according to NIST PQC standards.
/// </remarks>
public readonly struct MLDSA87Format : IOqsAlgorithmFormat
{
    /// <summary>
    /// The standardized algorithm name string "ML-DSA-87"
    /// </summary>
    /// <remarks>
    /// Corresponds to FIPS 204 ML-DSA specification with 87-bit security level.
    /// Represents Module-Lattice based Digital Signature Algorithm with enhanced security parameters.
    /// </remarks>
    private const string AlgorithmIdentifier = "ML-DSA-87";

    /// <summary>
    /// Gets the algorithm name identifier
    /// </summary>
    /// <value>
    /// Always returns "ML-DSA-87" string constant
    /// </value>
    public string Algorithm => AlgorithmIdentifier;

    /// <summary>
    /// Gets the signature length of 4627 bytes for ML-DSA-87
    /// </summary>
    public int SignatureLength => 4627;

    /// <summary>
    /// Gets the public key length of 2592 bytes for ML-DSA-87
    /// </summary>
    public int PublicKeyLength => 2592;

    /// <summary>
    /// Gets the private key length of 4896 bytes for ML-DSA-87
    /// </summary>
    public int PrivateKeyLength => 4896;
}
