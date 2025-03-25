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
    private static readonly object _libraryLock = new();
    private static readonly bool _libraryInitialized = false;

    private IntPtr _sig;
    private readonly object _instanceLock = new();
    private bool _disposed = false;

    private delegate IntPtr OQS_SIG_newDelegate(string alg_name);
    private delegate void OQS_SIG_freeDelegate(IntPtr sig);
    private delegate int OQS_SIG_keypairDelegate(IntPtr sig, byte[] public_key, byte[] private_key);
    private delegate int OQS_SIG_signDelegate(IntPtr sig, byte[] signature, out long signature_len, byte[] message, long message_len, byte[] private_key);
    private delegate int OQS_SIG_verifyDelegate(IntPtr sig, byte[] message, long message_len, byte[] signature, long signature_len, byte[] public_key);

    private static readonly OQS_SIG_newDelegate _oqsSigNew = null!;
    private static readonly OQS_SIG_freeDelegate _oqsSigFree = null!;
    private static readonly OQS_SIG_keypairDelegate _oqsSigKeypair = null!;
    private static readonly OQS_SIG_signDelegate _oqsSigSign = null!;
    private static readonly OQS_SIG_verifyDelegate _oqsSigVerify = null!;

    private static readonly string _dllPath = Path.Combine(Path.GetTempPath(), "oqs.dll");
    private static int _instanceCount = 0;

    private const string ResourceName = "Infrastructure.Assembly.oqs.dll";
    private const string AlgorithmName = "Dilithium3";
    private const int PublicKeyLength = 1952;
    private const int PrivateKeyLength = 4000;
    private const int SignatureLength = 3293;

    private static readonly Lazy<CryptographyException> _readingResourceError = new(() => new($"{ResourceName} is not found"));
    private static readonly Lazy<CryptographyException> _loadingError = new(() => new("Failed to load oqs.dll"));
    private static readonly Lazy<CryptographyException> _initializationError = new(() => new("Failed to initialize Dilithium"));
    private static readonly Lazy<CryptographyException> _generateKeyPairError = new(() => new("An error occurred while attempting to generate a key pair"));
    private static readonly Lazy<CryptographyException> _signingError = new(() => new("An error occurred while attempting to sign a message"));

    /// <summary>
    /// Static constructor to initialize the OQS library and resolve required functions.
    /// </summary>
    static DilithiumSigner()
    {
        lock (_libraryLock)
        {
            if (!_libraryInitialized)
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

                    _libraryInitialized = true;
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error during initialization: " + ex.ToString());
                    throw new CryptographyException(ex.Message);
                }
            }
        }
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="DilithiumSigner"/> class.
    /// </summary>
    public DilithiumSigner()
    {
        lock (_instanceLock)
        {
            _sig = _oqsSigNew(AlgorithmName);
            if (_sig == IntPtr.Zero)
                throw _initializationError.Value;

            Interlocked.Increment(ref _instanceCount);
        }
    }

    ~DilithiumSigner()
    {
        Dispose(false);
    }

    /// <summary>
    /// Generates a public/private key pair using the Dilithium algorithm.
    /// </summary>
    public (byte[] publicKey, byte[] privateKey) GenerateKeyPair()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        lock (_instanceLock)
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
    public byte[] Sign(byte[] message, byte[] privateKey)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        lock (_instanceLock)
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
    public bool Verify(byte[] message, byte[] signature, byte[] publicKey)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        lock (_instanceLock)
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

    protected virtual void Dispose(bool disposing)
    {
        if (_disposed)
            return;

        lock (_instanceLock)
        {
            if (_sig != IntPtr.Zero)
            {
                _oqsSigFree(_sig);
                _sig = IntPtr.Zero;
            }

            if (Interlocked.Decrement(ref _instanceCount) == 0 && _oqsLibraryHandle != IntPtr.Zero)
            {
                NativeLibrary.Free(_oqsLibraryHandle);
                _oqsLibraryHandle = IntPtr.Zero;

                if (File.Exists(_dllPath))
                {
                    try
                    {
                        File.Delete(_dllPath);
                    }
                    catch (IOException ex)
                    {
                        Console.WriteLine($"Failed to delete {_dllPath}: {ex.Message}");
                    }
                }
            }

            _disposed = true;
        }
    }
}
