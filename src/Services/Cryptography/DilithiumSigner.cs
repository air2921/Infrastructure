using Infrastructure.Abstractions;
using Infrastructure.Exceptions;
using System.Reflection;
using System.Runtime.InteropServices;

namespace Infrastructure.Services.Cryptography;

/// <summary>
/// A class for performing cryptographic operations using the Dilithium post-quantum signature algorithm.
/// This class implements the <see cref="ISigner"/> interface for signing and verifying messages,
/// and the <see cref="IDisposable"/> interface for resource management.
/// </summary>
/// <remarks>
/// This class uses the Open Quantum Safe (OQS) library to perform Dilithium-based cryptographic operations.
/// It loads the OQS library from an embedded resource, initializes the Dilithium algorithm, and provides
/// methods for generating key pairs, signing messages, and verifying signatures.
/// </remarks>
public class DilithiumSigner : ISigner, IDisposable
{
    private static IntPtr _oqsLibraryHandle;
    private IntPtr _sig;

    private delegate IntPtr OQS_SIG_newDelegate(string alg_name);
    private delegate void OQS_SIG_freeDelegate(IntPtr sig);
    private delegate int OQS_SIG_keypairDelegate(IntPtr sig, byte[] public_key, byte[] private_key);
    private delegate int OQS_SIG_signDelegate(IntPtr sig, byte[] signature, out long signature_len, byte[] message, long message_len, byte[] private_key);
    private delegate int OQS_SIG_verifyDelegate(IntPtr sig, byte[] message, long message_len, byte[] signature, long signature_len, byte[] public_key);

    private readonly static OQS_SIG_newDelegate _oqsSigNew;
    private readonly static OQS_SIG_freeDelegate _oqsSigFree;
    private readonly static OQS_SIG_keypairDelegate _oqsSigKeypair;
    private readonly static OQS_SIG_signDelegate _oqsSigSign;
    private readonly static OQS_SIG_verifyDelegate _oqsSigVerify;

    private readonly static string _dllPath = Path.Combine(Path.GetTempPath(), "oqs.dll");
    private readonly object _lock = new();
    private bool _disposed = false;

    private const string ResourceName = "Infrastructure.Assembly.oqs.dll";
    private const string AlgorithmName = "Dilithium3";
    private const int PublicKeyLength = 1952;
    private const int PrivateKeyLength = 4000;
    private const int SignatureLength = 3293;


    private static readonly Lazy<CryptographyException> _readingResourceError = new(() => new($"{ResourceName} is not found"));
    private static readonly Lazy<CryptographyException> _loadingError = new(() => new("Failed to load oqs.dll"));
    private static readonly Lazy<CryptographyException> _initializationError = new (() => new("Failed to initialize Dilithium"));
    private static readonly Lazy<CryptographyException> _generateKeyPairError = new(() => new("An error occurred while attempting to generate a key pair"));
    private static readonly Lazy<CryptographyException> _signingError = new(() => new("An error occurred while attempting to sign a message"));

    /// <summary>
    /// Static constructor to initialize the OQS library and resolve required functions.
    /// </summary>
    /// <exception cref="CryptographyException">Thrown if the OQS library fails to load or initialize.</exception>
    static DilithiumSigner()
    {
        try
        {
            Console.WriteLine($"Loading {ResourceName}...");

            using var assemblyStream = Assembly.GetExecutingAssembly().GetManifestResourceStream(ResourceName) ??
                throw _readingResourceError.Value;

            using (var fileStream = new FileStream(_dllPath, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None, 4096, true))
                assemblyStream.CopyTo(fileStream);

            _oqsLibraryHandle = NativeLibrary.Load(_dllPath);
            if (_oqsLibraryHandle == IntPtr.Zero)
                throw _loadingError.Value;

            Console.WriteLine("Resolving OQS_SIG_new...");
            _oqsSigNew = Marshal.GetDelegateForFunctionPointer<OQS_SIG_newDelegate>(
                NativeLibrary.GetExport(_oqsLibraryHandle, "OQS_SIG_new")
            );

            Console.WriteLine("Resolving OQS_SIG_free...");
            _oqsSigFree = Marshal.GetDelegateForFunctionPointer<OQS_SIG_freeDelegate>(
                NativeLibrary.GetExport(_oqsLibraryHandle, "OQS_SIG_free")
            );

            Console.WriteLine("Resolving OQS_SIG_keypair...");
            _oqsSigKeypair = Marshal.GetDelegateForFunctionPointer<OQS_SIG_keypairDelegate>(
                NativeLibrary.GetExport(_oqsLibraryHandle, "OQS_SIG_keypair")
            );

            Console.WriteLine("Resolving OQS_SIG_sign...");
            _oqsSigSign = Marshal.GetDelegateForFunctionPointer<OQS_SIG_signDelegate>(
                NativeLibrary.GetExport(_oqsLibraryHandle, "OQS_SIG_sign")
            );

            Console.WriteLine("Resolving OQS_SIG_verify...");
            _oqsSigVerify = Marshal.GetDelegateForFunctionPointer<OQS_SIG_verifyDelegate>(
                NativeLibrary.GetExport(_oqsLibraryHandle, "OQS_SIG_verify")
            );
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error during initialization: " + ex.ToString());
            throw new CryptographyException(ex.Message, ex);
        }
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="DilithiumSigner"/> class.
    /// </summary>
    /// <exception cref="CryptographyException">Thrown if the Dilithium algorithm fails to initialize.</exception>
    public DilithiumSigner()
    {
        _sig = _oqsSigNew(AlgorithmName);
        if (_sig == IntPtr.Zero)
            throw _initializationError.Value;
    }

    /// <summary>
    /// Finalizer to ensure resources are released if <see cref="Dispose()"/> is not called.
    /// </summary>
    ~DilithiumSigner()
    {
        Dispose(false);
    }

    /// <summary>
    /// Generates a public/private key pair using the Dilithium algorithm.
    /// </summary>
    /// <returns>A tuple containing the public key and private key as byte arrays.</returns>
    /// <exception cref="CryptographyException">Thrown if key pair generation fails.</exception>
    /// <example>
    /// <code>
    /// var signer = new DilithiumSigner();
    /// var (publicKey, privateKey) = signer.GenerateKeyPair();
    /// </code>
    /// </example>
    public (byte[] publicKey, byte[] privateKey) GenerateKeyPair()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        lock (_lock)
        {
            byte[] publicKey = new byte[PublicKeyLength];
            byte[] privateKey = new byte[PrivateKeyLength];

            int result = _oqsSigKeypair(_sig, publicKey, privateKey);
            if (result != 0)
                throw _generateKeyPairError.Value;

            return (publicKey, privateKey);
        }
    }

    /// <summary>
    /// Signs a message using the provided private key.
    /// </summary>
    /// <param name="message">The message to sign as a byte array.</param>
    /// <param name="privateKey">The private key to use for signing.</param>
    /// <returns>The signature as a byte array.</returns>
    /// <exception cref="CryptographyException">Thrown if signing fails.</exception>
    /// <example>
    /// <code>
    /// var signer = new DilithiumSigner();
    /// var signature = signer.Sign(message, privateKey);
    /// </code>
    /// </example>
    public byte[] Sign(byte[] message, byte[] privateKey)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        lock (_lock)
        {
            byte[] signature = new byte[SignatureLength];

            int result = _oqsSigSign(_sig, signature, out long signatureLen, message, message.Length, privateKey);
            if (result != 0)
                throw _signingError.Value;

            return signature;
        }
    }

    /// <summary>
    /// Verifies a message's signature using the provided public key.
    /// </summary>
    /// <param name="message">The original message as a byte array.</param>
    /// <param name="signature">The signature to verify as a byte array.</param>
    /// <param name="publicKey">The public key to use for verification.</param>
    /// <returns><c>true</c> if the signature is valid; otherwise, <c>false</c>.</returns>
    /// <example>
    /// <code>
    /// var signer = new DilithiumSigner();
    /// bool isValid = signer.Verify(message, signature, publicKey);
    /// </code>
    /// </example>
    public bool Verify(byte[] message, byte[] signature, byte[] publicKey)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        lock (_lock)
        {
            int result = _oqsSigVerify(_sig, message, message.Length, signature, signature.Length, publicKey);
            return result == 0;
        }
    }

    /// <summary>
    /// Releases all resources used by the <see cref="DilithiumSigner"/>.
    /// </summary>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Releases the unmanaged resources used by the <see cref="DilithiumSigner"/> and optionally releases the managed resources.
    /// </summary>
    /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
    protected virtual void Dispose(bool disposing)
    {
        if (_disposed)
            return;

        if (_sig != IntPtr.Zero)
        {
            _oqsSigFree(_sig);
            _sig = IntPtr.Zero;
        }

        if (_oqsLibraryHandle != IntPtr.Zero)
        {
            NativeLibrary.Free(_oqsLibraryHandle);
            _oqsLibraryHandle = IntPtr.Zero;
        }

        if (File.Exists(_dllPath))
            File.Delete(_dllPath);

        _disposed = true;
    }
}
