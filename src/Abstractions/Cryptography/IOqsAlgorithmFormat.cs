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
    public string ResourceName => DefineOqsResource();

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

    /// <summary>
    /// Determines the appropriate OQS (Open Quantum Safe) library resource based on the current operating system.
    /// </summary>
    /// <returns>The platform-specific OQS library filename.</returns>
    /// <exception cref="PlatformNotSupportedException">
    /// Thrown when the current platform is either OSX (temporarily unsupported) 
    /// or any other unsupported platform.
    /// </exception>
    private static string DefineOqsResource()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            return "Infrastructure.Assembly.oqs.dll";
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            return "Infrastructure.Assembly.oqs.so";
        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            throw new PlatformNotSupportedException("OSX platform is temporarily unsupported");

        throw new PlatformNotSupportedException("Oqs is not supported on this platform");
    }
}
