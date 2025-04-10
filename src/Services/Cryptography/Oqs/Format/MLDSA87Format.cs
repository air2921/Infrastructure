using Infrastructure.Abstractions.Cryptography;

namespace Infrastructure.Services.Cryptography.Oqs.Format;

/// <summary>
/// Provides format specifications for ML-DSA-87 (Module Lattice Digital Signature Algorithm 87)
/// </summary>
/// <remarks>
/// Implements parameters for the 87-bit security variant of ML-DSA according to NIST PQC standards.
/// </remarks>
public class MLDSA87Format : IOqsAlgorithmFormat
{
    /// <summary>
    /// Gets the algorithm name "ML-DSA-87" (FIPS 204 ML-DSA with 87-bit security)
    /// </summary>
    public string Algorithm => "ML-DSA-87";

    /// <summary>
    /// Gets the signature length of 2701 bytes for ML-DSA-87
    /// </summary>
    public int SignatureLength => 2701;

    /// <summary>
    /// Gets the public key length of 1472 bytes for ML-DSA-87
    /// </summary>
    public int PublicKeyLength => 1472;

    /// <summary>
    /// Gets the private key length of 3504 bytes for ML-DSA-87
    /// </summary>
    public int PrivateKeyLength => 3504;
}
