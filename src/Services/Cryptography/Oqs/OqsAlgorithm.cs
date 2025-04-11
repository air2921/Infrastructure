﻿using Infrastructure.Abstractions.Cryptography;
using Infrastructure.Exceptions;
using System.Reflection;
using System.Runtime.InteropServices;

namespace Infrastructure.Services.Cryptography.Oqs;

/// <summary>
/// Abstract base class implementing post-quantum cryptographic signature algorithms
/// using the Open Quantum Safe (OQS) library.
/// </summary>
/// <remarks>
/// <para>
/// This class provides a managed wrapper around the native OQS library functions for:
/// <list type="bullet">
/// <item><description>Key pair generation</description></item>
/// <item><description>Message signing</description></item>
/// <item><description>Signature verification</description></item>
/// </list>
/// </para>
/// 
/// <para>
/// The class implements the <see cref="IDisposable"/> pattern to properly manage native resources:
/// <list type="bullet">
/// <item><description>Automatically extracts and loads the native OQS DLL</description></item>
/// <item><description>Manages lifetime of native signature objects</description></item>
/// <item><description>Cleans up temporary files on process exit</description></item>
/// </list>
/// </para>
/// 
/// <para>
/// Thread safety is implemented through:
/// <list type="bullet">
/// <item><description><see cref="_libLock"/> for library initialization</description></item>
/// <item><description><see cref="pointerLock"/> for cryptographic operations</description></item>
/// <item><description>Volatile fields for cross-thread visibility</description></item>
/// </list>
/// </para>
/// 
/// <para>
/// Derived classes must:
/// <list type="bullet">
/// <item><description>Specify a valid <see cref="IOqsAlgorithmFormat"/></description></item>
/// <item><description>Implement algorithm-specific validation if needed</description></item>
/// <item><description>Optionally extend the disposal pattern</description></item>
/// </list>
/// </para>
/// 
/// <seealso cref="IDisposable"/>
/// <seealso cref="IOqsAlgorithmFormat"/>
/// </remarks>
public abstract class OqsAlgorithm : IDisposable
{
    /// <summary>
    /// The algorithm format specification containing parameters and metadata for the quantum-safe cryptographic algorithm.
    /// </summary>
    /// <remarks>
    /// This field holds an implementation of <see cref="IOqsAlgorithmFormat"/> that defines:
    /// - The algorithm name (e.g., "Dilithium2")
    /// - Key lengths (public and private)
    /// - Signature length
    /// - Resource name for the native library
    /// It is initialized during construction and used throughout the class for algorithm-specific operations.
    /// The format must pass validation via <see cref="IsValidFormat"/> before being used.
    /// </remarks>
    protected IOqsAlgorithmFormat algorithmFormat;

    /// <summary>
    /// The filename of the native library based on the current operating system platform.
    /// </summary>
    /// <remarks>
    /// This static readonly field determines the correct native library filename based on the runtime OS:
    /// - "oqs.so" for Linux platforms
    /// - "oqs.dll" for all other platforms (Windows, macOS etc.)
    /// The value is determined using <see cref="RuntimeInformation.IsOSPlatform"/> check during static initialization.
    /// This filename is used to construct the full library path in <see cref="_dllPath"/>.
    /// </remarks>
    private static readonly string _fileName = RuntimeInformation.IsOSPlatform(OSPlatform.Linux) ? "oqs.so" : "oqs.dll";

    /// <summary>
    /// The path to the temporary location where the <c>oqs.dll</c> library is stored.
    /// </summary>
    /// <remarks>
    /// This path is generated by joining the system's temporary directory with the filename <c>oqs.dll</c>.
    /// It is used to load the native <c>oqs.dll</c> library during the initialization of the class.
    /// </remarks>
    private static readonly string _dllPath = Path.Join(Path.GetTempPath(), _fileName);

    /// <summary>
    /// A flag indicating whether the instance has been disposed.
    /// </summary>
    /// <remarks>
    /// This field is used to track whether the object has been disposed, ensuring that no operations
    /// are performed on the instance after it has been disposed.
    /// </remarks>
    protected volatile bool disposed;

    #region Pointers

    /// <summary>
    /// A pointer to the loaded <c>oqs.dll</c> library.
    /// </summary>
    /// <remarks>
    /// This handle is used to interact with the native library and resolve function pointers for cryptographic operations.
    /// </remarks>
    private volatile IntPtr _oqsLibraryHandle;

    /// <summary>
    /// A pointer to the signature object created for the Dilithium algorithm.
    /// </summary>
    /// <remarks>
    /// This object is initialized during the class instantiation and is used to perform signing and verification operations.
    /// </remarks>
    protected volatile IntPtr sig;

    #endregion

    #region Delegates

    /// <summary>
    /// Delegate for the <c>OQS_SIG_new</c> function in the native library, used to initialize a new signature object.
    /// </summary>
    /// <remarks>
    /// This function is used to create an instance of the signature scheme for a specific algorithm.
    /// </remarks>
    private delegate IntPtr OQS_SIG_newDelegate(string alg_name);

    /// <summary>
    /// Delegate for the <c>OQS_SIG_free</c> function in the native library, used to free the signature object.
    /// </summary>
    /// <remarks>
    /// This function is used to release resources associated with a signature scheme instance.
    /// </remarks>
    private delegate void OQS_SIG_freeDelegate(IntPtr sig);

    /// <summary>
    /// Delegate for the <c>OQS_SIG_keypair</c> function in the native library, used to generate key pairs.
    /// </summary>
    /// <remarks>
    /// This function is used to generate a public/private key pair for the signature scheme.
    /// </remarks>
    protected delegate int OQS_SIG_keypairDelegate(IntPtr sig, byte[] public_key, byte[] private_key);

    /// <summary>
    /// Delegate for the <c>OQS_SIG_sign</c> function in the native library, used to sign a message.
    /// </summary>
    /// <remarks>
    /// This function is used to create a signature for a message using the private key.
    /// </remarks>
    protected delegate int OQS_SIG_signDelegate(IntPtr sig, byte[] signature, out long signature_len, byte[] message, long message_len, byte[] private_key);

    /// <summary>
    /// Delegate for the <c>OQS_SIG_verify</c> function in the native library, used to verify a signature.
    /// </summary>
    /// <remarks>
    /// This function is used to verify a signature against a message and public key.
    /// </remarks>
    protected delegate int OQS_SIG_verifyDelegate(IntPtr sig, byte[] message, long message_len, byte[] signature, long signature_len, byte[] public_key);

    /// <summary>
    /// The delegate for the <c>OQS_SIG_new</c> function, used for initializing the signature object.
    /// </summary>
    /// <remarks>
    /// This delegate is resolved at runtime and used to initialize a new signature instance for the Dilithium algorithm.
    /// </remarks>
    private OQS_SIG_newDelegate _oqsSigNew = null!;

    /// <summary>
    /// The delegate for the <c>OQS_SIG_free</c> function, used for freeing the signature object.
    /// </summary>
    /// <remarks>
    /// This delegate is resolved at runtime and used to release the resources of a signature instance.
    /// </remarks>
    private OQS_SIG_freeDelegate _oqsSigFree = null!;

    /// <summary>
    /// The delegate for the <c>OQS_SIG_keypair</c> function, used for generating key pairs.
    /// </summary>
    /// <remarks>
    /// This delegate is resolved at runtime and used to generate public and private key pairs for the Dilithium algorithm.
    /// </remarks>
    protected OQS_SIG_keypairDelegate oqsSigKeypair = null!;

    /// <summary>
    /// The delegate for the <c>OQS_SIG_sign</c> function, used for signing messages.
    /// </summary>
    /// <remarks>
    /// This delegate is resolved at runtime and used to create a signature for a message using the private key.
    /// </remarks>
    protected OQS_SIG_signDelegate oqsSigSign = null!;

    /// <summary>
    /// The delegate for the <c>OQS_SIG_verify</c> function, used for verifying signatures.
    /// </summary>
    /// <remarks>
    /// This delegate is resolved at runtime and used to verify the authenticity of a signature against a message.
    /// </remarks>
    protected OQS_SIG_verifyDelegate oqsSigVerify = null!;

    #endregion

    #region Lock Objects

    /// <summary>
    /// Lock object to synchronize access to the library handle during initialization and function resolution.
    /// </summary>
    /// <remarks>
    /// This object ensures that library loading and delegate resolution are thread-safe.
    /// </remarks>
    private readonly object _libLock = new();

    /// <summary>
    /// Lock object to synchronize access to the signature object during cryptographic operations.
    /// </summary>
    /// <remarks>
    /// This object ensures thread safety when performing signature operations such as signing and verification.
    /// </remarks>
    protected readonly object pointerLock = new();

    #endregion

    #region Constructor and Destructor

    /// <summary>
    /// Initializes a new instance of the <see cref="OqsAlgorithm{TAlgorithm}"/> class.
    /// Automatically creates and validates the algorithm format specification.
    /// </summary>
    /// <exception cref="CryptographyException">
    /// Thrown when:
    /// - The algorithm format fails validation (via <see cref="IsValidFormat"/>)
    /// - Failed to load or initialize the native OQS library
    /// - Failed to create the signature scheme instance
    /// </exception>
    /// <remarks>
    /// The constructor performs the following operations:
    /// <list type="number">
    ///   <item><description>Creates a new instance of <typeparamref name="TAlgorithm"/> using its parameterless constructor</description></item>
    ///   <item><description>Validates the algorithm format using <see cref="IsValidFormat"/></description></item>
    ///   <item><description>Stores the validated format in <see cref="algorithmFormat"/></description></item>
    ///   <item><description>Initializes native resources by calling <see cref="DilithiumPointerResolve"/></description></item>
    ///   <item><description>Registers a process exit handler to clean up the temporary DLL file</description></item>
    /// </list>
    /// 
    /// <para>
    /// Note: The temporary DLL file (<see cref="_dllPath"/>) is automatically deleted when the process exits.
    /// </para>
    /// 
    /// <para>
    /// Type Constraints:
    /// <typeparamref name="TAlgorithm"/> must have a parameterless constructor (enforced by <c>new()</c> constraint)
    /// and implement <see cref="IOqsAlgorithmFormat"/>.
    /// </para>
    /// </remarks>
    protected OqsAlgorithm(IOqsAlgorithmFormat algorithmFormat)
    {
        this.algorithmFormat = algorithmFormat;

        if (!IsValidFormat())
            throw new CryptographyException("Invalid algorithm format");

        DilithiumPointerResolve();

        AppDomain.CurrentDomain.ProcessExit += (sender, args) =>
        {
            if (File.Exists(_dllPath))
            {
                try
                {
                    File.Delete(_dllPath);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error deleting DLL file: " + ex.Message);
                }
            }
        };
    }

    /// <summary>
    /// Finalizes an instance of the <see cref="OqsAlgorithm"/> class.
    /// </summary>
    /// <remarks>
    /// This destructor ensures that unmanaged resources are properly released
    /// if the object was not explicitly disposed. It calls <see cref="Dispose(bool)"/>
    /// with <c>false</c> to perform cleanup of unmanaged resources only.
    /// 
    /// Note: The destructor is only called if <see cref="Dispose()"/> was not called.
    /// </remarks>
    ~OqsAlgorithm()
    {
        Dispose(false);
    }

    #endregion

    #region Resolvers

    /// <summary>
    /// Initializes and resolves all required native function pointers for the OQS algorithm.
    /// </summary>
    /// <exception cref="CryptographyException">
    /// Thrown when:
    /// - The embedded DLL resource cannot be found or extracted
    /// - The native library fails to load
    /// - The signature scheme initialization fails
    /// </exception>
    /// <remarks>
    /// This method performs the following operations in a thread-safe manner:
    /// 1. Extracts the embedded OQS DLL to a temporary file if not already present
    /// 2. Loads the native library using <see cref="NativeLibrary.Load"/>
    /// 3. Resolves all required function delegates via <see cref="ResolveDelegates"/>
    /// 4. Initializes the signature scheme instance
    /// 
    /// The operation is protected by <see cref="_libLock"/> to ensure thread safety.
    /// Any errors during this process are wrapped in a <see cref="CryptographyException"/>.
    /// </remarks>
    protected virtual void DilithiumPointerResolve()
    {
        lock (_libLock)
        {
            try
            {
                if (!File.Exists(_dllPath))
                {
                    using var assemblyStream = Assembly.GetExecutingAssembly().GetManifestResourceStream(algorithmFormat.ResourceName) ??
                        throw new CryptographyException($"{algorithmFormat.ResourceName} is not found");

#pragma warning disable IDE0063
                    using (var stream = new FileStream(_dllPath, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None, 4096))
                        assemblyStream.CopyTo(stream);
#pragma warning restore IDE0063
                }

                _oqsLibraryHandle = NativeLibrary.Load(_dllPath);
                if (_oqsLibraryHandle == IntPtr.Zero)
                    throw new CryptographyException("Failed to load oqs.dll");

                ResolveDelegates();

                sig = _oqsSigNew(algorithmFormat.Algorithm);
                if (sig == IntPtr.Zero)
                    throw new CryptographyException($"Failed to initialize {algorithmFormat.Algorithm}");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error during initialization: " + ex.ToString());
                throw new CryptographyException(ex.Message);
            }
        }
    }

    /// <summary>
    /// Resolves all native function delegates required for cryptographic operations.
    /// </summary>
    /// <remarks>
    /// This method sequentially resolves the following function pointers:
    /// 1. Signature creation (<see cref="ResolveNewSignatureDelegate"/>)
    /// 2. Signature cleanup (<see cref="ResolveSignatureFreeDelegate"/>)
    /// 3. Key pair generation (<see cref="ResolveGenerateKeyPairDelegate"/>)
    /// 4. Message signing (<see cref="ResolveSignDelegate"/>)
    /// 5. Signature verification (<see cref="ResolveVerifyDelegate"/>)
    /// </remarks>
    protected virtual void ResolveDelegates()
    {
        ResolveNewSignatureDelegate();
        ResolveSignatureFreeDelegate();
        ResolveGenerateKeyPairDelegate();
        ResolveSignDelegate();
        ResolveVerifyDelegate();
    }

    /// <summary>
    /// Resolves the delegate for the OQS_SIG_new function.
    /// </summary>
    /// <remarks>
    /// The resolved delegate is stored in <see cref="_oqsSigNew"/> and is used to create
    /// new instances of the signature scheme. This corresponds to the native function:
    /// <code>OQS_SIG* OQS_SIG_new(const char* alg_name);</code>
    /// </remarks>
    private void ResolveNewSignatureDelegate()
    {
        Console.WriteLine("Resolving OQS_SIG_new...");
        _oqsSigNew = Marshal.GetDelegateForFunctionPointer<OQS_SIG_newDelegate>(
            NativeLibrary.GetExport(_oqsLibraryHandle, "OQS_SIG_new")
        );
    }

    /// <summary>
    /// Resolves the delegate for the OQS_SIG_free function.
    /// </summary>
    /// <remarks>
    /// The resolved delegate is stored in <see cref="_oqsSigFree"/> and is used to
    /// clean up signature scheme instances. This corresponds to the native function:
    /// <code>void OQS_SIG_free(OQS_SIG* sig);</code>
    /// </remarks>
    private void ResolveSignatureFreeDelegate()
    {
        Console.WriteLine("Resolving OQS_SIG_free...");
        _oqsSigFree = Marshal.GetDelegateForFunctionPointer<OQS_SIG_freeDelegate>(
            NativeLibrary.GetExport(_oqsLibraryHandle, "OQS_SIG_free")
        );
    }

    /// <summary>
    /// Resolves the delegate for the OQS_SIG_keypair function.
    /// </summary>
    /// <remarks>
    /// The resolved delegate is stored in <see cref="oqsSigKeypair"/> and is used to
    /// generate public/private key pairs. This corresponds to the native function:
    /// <code>int OQS_SIG_keypair(const OQS_SIG* sig, uint8_t* public_key, uint8_t* private_key);</code>
    /// </remarks>
    private void ResolveGenerateKeyPairDelegate()
    {
        Console.WriteLine("Resolving OQS_SIG_keypair...");
        oqsSigKeypair = Marshal.GetDelegateForFunctionPointer<OQS_SIG_keypairDelegate>(
            NativeLibrary.GetExport(_oqsLibraryHandle, "OQS_SIG_keypair")
        );
    }

    /// <summary>
    /// Resolves the delegate for the OQS_SIG_sign function.
    /// </summary>
    /// <remarks>
    /// The resolved delegate is stored in <see cref="oqsSigSign"/> and is used to
    /// create message signatures. This corresponds to the native function:
    /// <code>int OQS_SIG_sign(const OQS_SIG* sig, uint8_t* signature, size_t* signature_len,
    /// const uint8_t* message, size_t message_len, const uint8_t* private_key);</code>
    /// </remarks>
    private void ResolveSignDelegate()
    {
        Console.WriteLine("Resolving OQS_SIG_sign...");
        oqsSigSign = Marshal.GetDelegateForFunctionPointer<OQS_SIG_signDelegate>(
            NativeLibrary.GetExport(_oqsLibraryHandle, "OQS_SIG_sign")
        );
    }

    /// <summary>
    /// Resolves the delegate for the OQS_SIG_verify function.
    /// </summary>
    /// <remarks>
    /// The resolved delegate is stored in <see cref="oqsSigVerify"/> and is used to
    /// verify message signatures. This corresponds to the native function:
    /// <code>int OQS_SIG_verify(const OQS_SIG* sig, const uint8_t* message, size_t message_len,
    /// const uint8_t* signature, size_t signature_len, const uint8_t* public_key);</code>
    /// </remarks>
    private void ResolveVerifyDelegate()
    {
        Console.WriteLine("Resolving OQS_SIG_verify...");
        oqsSigVerify = Marshal.GetDelegateForFunctionPointer<OQS_SIG_verifyDelegate>(
            NativeLibrary.GetExport(_oqsLibraryHandle, "OQS_SIG_verify")
        );
    }

    #endregion

    #region Param Validators

    /// <summary>
    /// Validates the algorithm format parameters.
    /// </summary>
    /// <returns>
    /// <c>true</c> if the format meets all validation requirements; otherwise, <c>false</c>.
    /// </returns>
    /// <remarks>
    /// This method performs comprehensive validation by checking:
    /// 1. Signature length validity via <see cref="IsValidSignatureLength"/>
    /// 2. Key length validity via <see cref="IsValidKeyLength"/>
    /// 
    /// The method is marked as virtual to allow derived classes to extend the validation logic.
    /// </remarks>
    protected virtual bool IsValidFormat()
    {
        if (!IsValidSignatureLength())
            return false;

        if (!IsValidKeyLength())
            return false;

        return true;
    }

    /// <summary>
    /// Validates the key lengths specified in the algorithm format.
    /// </summary>
    /// <returns>
    /// <c>true</c> if both public and private key lengths are non-zero; otherwise, <c>false</c>.
    /// </returns>
    /// <remarks>
    /// Ensures the algorithm format specifies valid lengths for:
    /// - Public key (<see cref="IOqsAlgorithmFormat.PublicKeyLength"/>)
    /// - Private key (<see cref="IOqsAlgorithmFormat.PrivateKeyLength"/>)
    /// </remarks>
    private bool IsValidKeyLength()
    {
        if (algorithmFormat.PublicKeyLength == 0 || algorithmFormat.PrivateKeyLength == 0)
            return false;

        return true;
    }

    /// <summary>
    /// Validates the signature length specified in the algorithm format.
    /// </summary>
    /// <returns>
    /// <c>true</c> if the signature length is non-zero; otherwise, <c>false</c>.
    /// </returns>
    /// <remarks>
    /// Checks that <see cref="IOqsAlgorithmFormat.SignatureLength"/> contains a valid positive value.
    /// </remarks>
    private bool IsValidSignatureLength()
    {
        if (algorithmFormat.SignatureLength == 0)
            return false;

        return true;
    }

    #endregion

    #region Disposable

    /// <summary>
    /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
    /// </summary>
    /// <param name="disposing">
    /// <c>true</c> to release both managed and unmanaged resources;
    /// <c>false</c> to release only unmanaged resources.
    /// </param>
    /// <remarks>
    /// This method implements the standard dispose pattern and performs the following operations:
    /// 1. Checks the disposal state via <see cref="_disposed"/> flag to prevent duplicate disposal
    /// 2. Uses <see cref="pointerLock"/> to ensure thread-safe cleanup
    /// 3. Releases the signature object via <see cref="_oqsSigFree"/> if <see cref="_sig"/> is initialized
    /// 4. Conditionally frees the native library handle (<see cref="_oqsLibraryHandle"/>) when <paramref name="disposing"/> is true
    /// 5. Sets the <see cref="_disposed"/> flag to prevent future operations on disposed instance
    ///
    /// Note: When called with <c>false</c> (from finalizer), only unmanaged resources are released.
    /// Derived classes should override this method to add their own cleanup logic while calling base.Dispose(disposing).
    /// </remarks>
    protected virtual void Dispose(bool disposing)
    {
        if (disposed)
            return;

        lock (pointerLock)
        {
            if (sig != IntPtr.Zero)
            {
                _oqsSigFree(sig);
                sig = IntPtr.Zero;
            }

            if (disposing)
            {
                if (sig != IntPtr.Zero)
                {
                    NativeLibrary.Free(_oqsLibraryHandle);
                    _oqsLibraryHandle = IntPtr.Zero;
                }
            }
        }

        disposed = true;
    }

    /// <summary>
    /// Releases all resources used by the current instance of <see cref="OqsAlgorithm"/>.
    /// </summary>
    /// <remarks>
    /// This method:
    /// 1. Calls <see cref="Dispose(bool)"/> with <c>true</c> to release all resources
    /// 2. Suppresses finalization via <see cref="GC.SuppressFinalize"/> to prevent redundant cleanup
    /// 3. Should be called explicitly when the instance is no longer needed
    ///
    /// After calling Dispose(), the object should not be used as it may leave the instance in an unusable state.
    /// </remarks>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Verifies that the instance has not been disposed.
    /// </summary>
    /// <exception cref="ObjectDisposedException">
    /// Thrown when the instance has already been disposed.
    /// </exception>
    /// <remarks>
    /// <para>
    /// This method should be called at the start of any public method or property that requires
    /// access to the instance's resources.
    /// </para>
    /// <para>
    /// The check is performed by verifying the <see cref="_disposed"/> flag. If the flag is set to <c>true</c>,
    /// the method throws an <see cref="ObjectDisposedException"/> with the current instance's type name.
    /// </para>
    /// </remarks>
    protected void CheckDisposed()
        => ObjectDisposedException.ThrowIf(disposed, this);

    #endregion
}
