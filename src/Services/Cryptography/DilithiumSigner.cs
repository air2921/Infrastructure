using Infrastructure.Abstractions.Cryptography;
using Infrastructure.Exceptions;
using System.Reflection;
using System.Runtime.InteropServices;

namespace Infrastructure.Services.Cryptography;

/// <summary>
/// A thread-safe class for performing cryptographic operations using the Dilithium post-quantum signature algorithm.
/// This class implements the <see cref="ISigner"/> interface for signing and verifying messages,
/// and the <see cref="IDisposable"/> interface for resource management.
/// </summary>
/// <remarks>
/// <para>
/// This class uses the Open Quantum Safe (OQS) library to perform Dilithium-based cryptographic operations.
/// It loads the OQS library from an embedded resource, initializes the Dilithium algorithm, and provides
/// methods for generating key pairs, signing messages, and verifying signatures.
/// </para>
/// <para>
/// The class implements proper resource cleanup through both the <see cref="IDisposable"/> pattern
/// and finalizer. The native resources are automatically freed when the instance is disposed or
/// garbage collected.
/// </para>
/// <para>
/// <strong>Thread Safety:</strong> All public members of this class are thread-safe and may be used
/// concurrently from multiple threads. The internal synchronization ensures correct operation
/// in multi-threaded scenarios.
/// </para>
/// <para>
/// <strong>Resource Management:</strong> The native OQS library is loaded only once and shared between
/// all instances. It is automatically unloaded when the last instance is disposed. The temporary DLL
/// file is automatically deleted when no longer needed.
/// </para>
/// </remarks>
public class DilithiumSigner : ISigner, IDisposable
{
    private static IntPtr _oqsLibraryHandle;
    private static readonly object _libraryLock = new();
    private static volatile bool _libraryInitialized = false;

    private IntPtr _sig;
    private readonly object _instanceLock = new();
    private volatile bool _disposed = false;

    private delegate IntPtr OQS_SIG_newDelegate(string alg_name);
    private delegate void OQS_SIG_freeDelegate(IntPtr sig);
    private delegate int OQS_SIG_keypairDelegate(IntPtr sig, byte[] public_key, byte[] private_key);
    private delegate int OQS_SIG_signDelegate(IntPtr sig, byte[] signature, out long signature_len, byte[] message, long message_len, byte[] private_key);
    private delegate int OQS_SIG_verifyDelegate(IntPtr sig, byte[] message, long message_len, byte[] signature, long signature_len, byte[] public_key);

    private static OQS_SIG_newDelegate _oqsSigNew = null!;
    private static OQS_SIG_freeDelegate _oqsSigFree = null!;
    private static OQS_SIG_keypairDelegate _oqsSigKeypair = null!;
    private static OQS_SIG_signDelegate _oqsSigSign = null!;
    private static OQS_SIG_verifyDelegate _oqsSigVerify = null!;

    private static readonly string _dllPath = Path.Combine(Path.GetTempPath(), "oqs.dll");
    private static volatile int _instanceCount = 0;

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
    /// Static constructor that initializes the OQS library when the first instance of <see cref="DilithiumSigner"/> is created.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This constructor performs one-time initialization of the native OQS library using double-checked locking
    /// for thread safety. The initialization includes:
    /// </para>
    /// <list type="bullet">
    /// <item><description>Extracting the embedded OQS DLL to a temporary file</description></item>
    /// <item><description>Loading the native library into the process</description></item>
    /// <item><description>Resolving all required function pointers for cryptographic operations</description></item>
    /// </list>
    /// <para>
    /// If initialization fails, a <see cref="CryptographyException"/> will be thrown, and subsequent attempts
    /// to create instances will fail with the same exception.
    /// </para>
    /// </remarks>
    /// <exception cref="CryptographyException">
    /// Thrown when library initialization fails for any reason, including:
    /// <list type="bullet">
    /// <item><description>Missing embedded resource</description></item>
    /// <item><description>Failure to load the native library</description></item>
    /// <item><description>Failure to resolve required function exports</description></item>
    /// </list>
    /// </exception>
    static DilithiumSigner()
    {
        InitializeLibrary();
    }

    /// <summary>
    /// Initializes the OQS library and resolves all required function pointers.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This method implements thread-safe lazy initialization using the double-checked locking pattern.
    /// It performs the following operations exactly once, regardless of how many threads attempt initialization:
    /// </para>
    /// <list type="number">
    /// <item><description>Extracts the embedded OQS DLL (<see cref="ResourceName"/>) to a temporary file (<see cref="_dllPath"/>)</description></item>
    /// <item><description>Loads the native library using <see cref="NativeLibrary.Load"/></description></item>
    /// <item><description>Resolves the following required functions:
    ///     <list type="bullet">
    ///     <item><description><c>OQS_SIG_new</c> - Creates a new signature scheme instance</description></item>
    ///     <item><description><c>OQS_SIG_free</c> - Releases a signature scheme instance</description></item>
    ///     <item><description><c>OQS_SIG_keypair</c> - Generates key pairs</description></item>
    ///     <item><description><c>OQS_SIG_sign</c> - Signs messages</description></item>
    ///     <item><description><c>OQS_SIG_verify</c> - Verifies signatures</description></item>
    ///     </list>
    /// </description></item>
    /// </list>
    /// <para>
    /// The temporary DLL file is marked for deletion when the process exits, but will be explicitly deleted
    /// when the last <see cref="DilithiumSigner"/> instance is disposed.
    /// </para>
    /// <para>
    /// Debugging information is written to the console during initialization to help diagnose any issues.
    /// </para>
    /// </remarks>
    /// <exception cref="CryptographyException">
    /// Thrown when any step of the initialization process fails. The exception will contain
    /// details about the specific failure.
    /// </exception>
    private static void InitializeLibrary()
    {
        if (_libraryInitialized)
            return;

        lock (_libraryLock)
        {
            if (_libraryInitialized)
                return;

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

    /// <summary>
    /// Initializes a new instance of the <see cref="DilithiumSigner"/> class.
    /// </summary>
    /// <exception cref="CryptographyException">
    /// Thrown when:
    /// <list type="bullet">
    /// <item><description>The embedded OQS library resource cannot be found</description></item>
    /// <item><description>The native library fails to load</description></item>
    /// <item><description>The Dilithium algorithm initialization fails</description></item>
    /// </list>
    /// </exception>
    /// <remarks>
    /// This constructor initializes the underlying OQS library if it hasn't been initialized yet.
    /// The initialization is thread-safe and performs lazy loading of the native library.
    /// </remarks>
    public DilithiumSigner()
    {
        InitializeLibrary();

        lock (_instanceLock)
        {
            _sig = _oqsSigNew(AlgorithmName);
            if (_sig == IntPtr.Zero)
                throw _initializationError.Value;

            Interlocked.Increment(ref _instanceCount);
        }
    }

    /// <summary>
    /// Finalizer that ensures native resources are released if Dispose was not called.
    /// </summary>
    /// <remarks>
    /// The finalizer only releases the native signature context but does not unload the OQS library
    /// or delete the temporary file, as these operations should be performed deterministically.
    /// </remarks>
    ~DilithiumSigner()
    {
        Dispose(false);
    }

    /// <summary>
    /// Generates a new public/private key pair using the Dilithium3 algorithm.
    /// </summary>
    /// <returns>
    /// A tuple containing the public key (1952 bytes) and private key (4000 bytes).
    /// </returns>
    /// <exception cref="ObjectDisposedException">
    /// Thrown if the instance has been disposed.
    /// <remarks>
    /// The key generation is thread-safe and can be called concurrently from multiple threads.
    /// </remarks>
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
    /// <param name="message">The message to sign.</param>
    /// <param name="privateKey">The private key (must be 4000 bytes).</param>
    /// <returns>
    /// The signature (3293 bytes) for the message.
    /// </returns>
    /// <exception cref="ObjectDisposedException">
    /// Thrown if the instance has been disposed.
    /// </exception>
    /// <exception cref="CryptographyException">
    /// Thrown if the signing operation fails.
    /// </exception>
    /// <remarks>
    /// The signing operation is thread-safe and can be called concurrently from multiple threads.
    /// </remarks>
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
    /// <param name="message">The original message.</param>
    /// <param name="signature">The signature to verify (must be 3293 bytes).</param>
    /// <param name="publicKey">The public key (must be 1952 bytes).</param>
    /// <returns>
    /// <c>true</c> if the signature is valid; otherwise, <c>false</c>.
    /// </returns>
    /// <exception cref="ObjectDisposedException">
    /// Thrown if the instance has been disposed.
    /// </exception>
    /// <remarks>
    /// The verification operation is thread-safe and can be called concurrently from multiple threads.
    /// </remarks>
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
    /// <remarks>
    /// <para>
    /// This method releases the native resources associated with this instance. If this is the last
    /// active instance, it also unloads the OQS library and deletes the temporary DLL file.
    /// </para>
    /// <para>
    /// The object should not be used after being disposed. Always call Dispose when you are finished
    /// using the DilithiumSigner, preferably by wrapping it in a using statement.
    /// </para>
    /// </remarks>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Releases the unmanaged resources used by the <see cref="DilithiumSigner"/> and optionally releases the managed resources.
    /// </summary>
    /// <param name="disposing">
    /// <c>true</c> to release both managed and unmanaged resources; 
    /// <c>false</c> to release only unmanaged resources.
    /// </param>
    /// <remarks>
    /// <para>
    /// This method performs the following cleanup operations in a thread-safe manner:
    /// </para>
    /// <list type="number">
    /// <item>
    /// <description>
    /// Always releases the native signature context (<c>_sig</c>) by calling <c>OQS_SIG_free</c>,
    /// regardless of the <paramref name="disposing"/> parameter value.
    /// </description>
    /// </item>
    /// <item>
    /// <description>
    /// When <paramref name="disposing"/> is <c>true</c> (regular disposal):
    /// <list type="bullet">
    ///   <item><description>Decrements the instance counter</description></item>
    ///   <item><description>If this is the last instance, unloads the OQS native library using <see cref="NativeLibrary.Free"/></description></item>
    ///   <item><description>Attempts to delete the temporary DLL file (<see cref="_dllPath"/>)</description></item>
    /// </list>
    /// </description>
    /// </item>
    /// <item>
    /// <description>
    /// When <paramref name="disposing"/> is <c>false</c> (finalizer context):
    /// Only releases the native signature context, but does not attempt to unload the library
    /// or delete the temporary file, as these operations require managed resources.
    /// </description>
    /// </item>
    /// </list>
    /// <para>
    /// The method is idempotent - multiple calls will have no effect after the first call.
    /// All operations are protected by <see cref="_instanceLock"/> to ensure thread safety.
    /// </para>
    /// <para>
    /// If file deletion fails, the error is logged to the console but not propagated,
    /// as disposal should not throw exceptions.
    /// </para>
    /// </remarks>
    protected virtual void Dispose(bool disposing)
    {
        if (_disposed)
            return;

        lock (_instanceLock)
        {
            if (_disposed)
                return;

            if (_sig != IntPtr.Zero)
            {
                _oqsSigFree(_sig);
                _sig = IntPtr.Zero;
            }

            if (disposing && Interlocked.Decrement(ref _instanceCount) == 0 && _oqsLibraryHandle != IntPtr.Zero)
            {
                NativeLibrary.Free(_oqsLibraryHandle);
                _oqsLibraryHandle = IntPtr.Zero;

                try
                {
                    if (File.Exists(_dllPath))
                        File.Delete(_dllPath);
                }
                catch (IOException ex)
                {
                    Console.WriteLine($"Failed to delete {_dllPath}: {ex.Message}");
                }
            }

            _disposed = true;
        }
    }
}
