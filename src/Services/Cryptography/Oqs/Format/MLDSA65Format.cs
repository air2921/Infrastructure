﻿using Infrastructure.Abstractions.Cryptography;

namespace Infrastructure.Services.Cryptography.Oqs.Format;

/// <summary>
/// Provides format specifications for ML-DSA-65 (Module Lattice Digital Signature Algorithm 65) 
/// </summary>
/// <remarks>
/// Implements parameters for the 65-bit security variant of ML-DSA according to NIST PQC standards.
/// </remarks>
public class MLDSA65Format : IOqsAlgorithmFormat
{
    /// <summary>
    /// Gets the algorithm name "ML-DSA-65" (FIPS 204 ML-DSA with 65-bit security)
    /// </summary>
    public string Algorithm => "ML-DSA-65";

    /// <summary>
    /// Gets the signature length of 2138 bytes for ML-DSA-65
    /// </summary>
    public int SignatureLength => 2138;

    /// <summary>
    /// Gets the public key length of 1184 bytes for ML-DSA-65
    /// </summary>
    public int PublicKeyLength => 1184;

    /// <summary>
    /// Gets the private key length of 2800 bytes for ML-DSA-65
    /// </summary>
    public int PrivateKeyLength => 2800;
}
