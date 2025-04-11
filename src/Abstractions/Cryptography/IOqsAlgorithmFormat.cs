using System.Runtime.InteropServices;

namespace Infrastructure.Abstractions.Cryptography;

/// <summary>
/// Specifies the format and parameters for a quantum-safe cryptographic algorithm
/// </summary>
/// <remarks>
/// Default implementation includes embedded OQS library path.
/// </remarks>
public interface IOqsAlgorithmFormat
{
    /// <summary>
    /// Path to embedded OQS library
    /// </summary>
    public string ResourceName => RuntimeInformation.IsOSPlatform(OSPlatform.Linux) ? "Infrastructure.Assembly.oqs.so" : "Infrastructure.Assembly.oqs.dll";

    /// <summary>
    /// Algorithm name (e.g. "Dilithium5")
    /// </summary>
    public string Algorithm { get; }

    /// <summary>
    /// Signature length in bytes
    /// </summary>
    public int SignatureLength { get; }

    /// <summary>
    /// Public key length in bytes 
    /// </summary>
    public int PublicKeyLength { get; }

    /// <summary>
    /// Private key length in bytes
    /// </summary>
    public int PrivateKeyLength { get; }
}
