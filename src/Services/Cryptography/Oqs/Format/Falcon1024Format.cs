using Infrastructure.Abstractions.Cryptography;

namespace Infrastructure.Services.Cryptography.Oqs.Format;

/// <summary>
/// Provides format specifications for Falcon-1024 (Fast-Fourier Lattice-based Compact Signatures)
/// </summary>
/// <remarks>
/// Implements parameters for the 1024-bit security variant of Falcon according to NIST PQC standards.
/// </remarks>
public class Falcon1024Format : IOqsAlgorithmFormat
{
    /// <summary>
    /// Gets the algorithm name "Falcon-1024" (FIPS 205 SLH-DSA with 1024-bit security)
    /// </summary>
    public string Algorithm => "Falcon-1024";

    /// <summary>
    /// Gets the signature length of 1280 bytes for Falcon-1024
    /// </summary>
    public int SignatureLength => 1280;

    /// <summary>
    /// Gets the public key length of 1793 bytes for Falcon-1024
    /// </summary>
    public int PublicKeyLength => 1793;

    /// <summary>
    /// Gets the private key length of 2305 bytes for Falcon-1024
    /// </summary>
    public int PrivateKeyLength => 2305;
}
