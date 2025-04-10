namespace Infrastructure.Abstractions.Cryptography;

/// <summary>
/// Specifies the format and parameters for a quantum-safe cryptographic algorithm
/// </summary>
/// <remarks>
/// Provides required parameters and factory method for concrete algorithm implementations.
/// Default implementation includes embedded OQS library path.
/// </remarks>
public interface IOqsAlgorithmFormat
{
    /// <summary>
    /// Path to embedded OQS library (default: "Infrastructure.Assembly.oqs.dll")
    /// </summary>
    public string ResourceName => "Infrastructure.Assembly.oqs.dll";

    /// <summary>
    /// Algorithm name (e.g. "Dilithium2")
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
