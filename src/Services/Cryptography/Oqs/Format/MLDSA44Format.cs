using Infrastructure.Abstractions.Cryptography;

namespace Infrastructure.Services.Cryptography.Oqs.Format;

/// <summary>
/// Provides format specifications for ML-DSA-44 (Module Lattice Digital Signature Algorithm 44)
/// </summary>
/// <remarks>
/// Implements parameters for the 44-bit security variant of ML-DSA according to NIST PQC standards.
/// </remarks>
public class MLDSA44Format : IOqsAlgorithmFormat
{
    /// <summary>
    /// Gets the algorithm name "ML-DSA-44" (FIPS 204 ML-DSA with 44-bit security)
    /// </summary>
    public string Algorithm => "ML-DSA-44";

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
