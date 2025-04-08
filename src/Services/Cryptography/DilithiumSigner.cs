﻿using Infrastructure.Abstractions.Cryptography;
using Infrastructure.Data_Transfer_Object.Cryptography;
using Infrastructure.Exceptions;
using System.Reflection;
using System.Runtime.InteropServices;

namespace Infrastructure.Services.Cryptography;

/// <summary>
/// Provides functionality for signing and verifying messages using the Dilithium3 post-quantum signature algorithm.
/// </summary>
/// <remarks>
/// This class utilizes the native <c>oqs.dll</c> library to perform cryptographic operations, such as key generation, 
/// signing, and signature verification. The library is loaded dynamically at runtime, and the necessary function pointers 
/// are resolved to allow interaction with the Dilithium algorithm. The class is thread-safe for cryptographic operations, 
/// and all resources are properly managed to avoid memory leaks.
/// </remarks>
/// <exception cref="CryptographyException">
/// Thrown if any error occurs while interacting with the native library or performing cryptographic operations.
/// </exception>
public class DilithiumSigner : ISigner, IDisposable
{
    /// <summary>
    /// The path to the temporary location where the <c>oqs.dll</c> library is stored.
    /// </summary>
    /// <remarks>
    /// This path is generated by joining the system's temporary directory with the filename <c>oqs.dll</c>.
    /// It is used to load the native <c>oqs.dll</c> library during the initialization of the class.
    /// </remarks>
    private static readonly string _dllPath = Path.Join(Path.GetTempPath(), "oqs.dll");

    /// <summary>
    /// A flag indicating whether the instance has been disposed.
    /// </summary>
    /// <remarks>
    /// This field is used to track whether the object has been disposed, ensuring that no operations
    /// are performed on the instance after it has been disposed.
    /// </remarks>
    private volatile bool disposed;

    #region Descriptors

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
    private volatile IntPtr _sig;

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
    private delegate int OQS_SIG_keypairDelegate(IntPtr sig, byte[] public_key, byte[] private_key);

    /// <summary>
    /// Delegate for the <c>OQS_SIG_sign</c> function in the native library, used to sign a message.
    /// </summary>
    /// <remarks>
    /// This function is used to create a signature for a message using the private key.
    /// </remarks>
    private delegate int OQS_SIG_signDelegate(IntPtr sig, byte[] signature, out long signature_len, byte[] message, long message_len, byte[] private_key);

    /// <summary>
    /// Delegate for the <c>OQS_SIG_verify</c> function in the native library, used to verify a signature.
    /// </summary>
    /// <remarks>
    /// This function is used to verify a signature against a message and public key.
    /// </remarks>
    private delegate int OQS_SIG_verifyDelegate(IntPtr sig, byte[] message, long message_len, byte[] signature, long signature_len, byte[] public_key);

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
    private OQS_SIG_keypairDelegate _oqsSigKeypair = null!;

    /// <summary>
    /// The delegate for the <c>OQS_SIG_sign</c> function, used for signing messages.
    /// </summary>
    /// <remarks>
    /// This delegate is resolved at runtime and used to create a signature for a message using the private key.
    /// </remarks>
    private OQS_SIG_signDelegate _oqsSigSign = null!;

    /// <summary>
    /// The delegate for the <c>OQS_SIG_verify</c> function, used for verifying signatures.
    /// </summary>
    /// <remarks>
    /// This delegate is resolved at runtime and used to verify the authenticity of a signature against a message.
    /// </remarks>
    private OQS_SIG_verifyDelegate _oqsSigVerify = null!;

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
    private readonly object _pointerLock = new();

    #endregion

    #region Constructor and Destructor

    /// <summary>
    /// Initializes a new instance of the <see cref="DilithiumSigner"/> class.
    /// </summary>
    /// <remarks>
    /// This constructor attempts to resolve the required function pointers from the native <c>oqs.dll</c> library.
    /// It ensures the proper loading of the native library and initialization of the signature object for the Dilithium algorithm.
    /// Additionally, it subscribes to the <c>ProcessExit</c> event to delete the temporary <c>oqs.dll</c> file when the application terminates,
    /// ensuring the cleanup of resources even if <see cref="Dispose"/> is not explicitly called.
    /// </remarks>
    public DilithiumSigner()
    {
        DilithiumPointerResolve();

        AppDomain.CurrentDomain.ProcessExit += (sender, args) =>
        {
            if (!File.Exists(_dllPath))
                return;

            try
            {
                File.Delete(_dllPath);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error deleting DLL file: " + ex.Message);
            }
        };
    }

    /// <summary>
    /// Finalizer for the <see cref="DilithiumSigner"/> class.
    /// </summary>
    /// <remarks>
    /// This destructor ensures that the resources are released when the object is garbage collected,
    /// in case <see cref="Dispose"/> was not explicitly called.
    /// </remarks>
    ~DilithiumSigner()
    {
        Dispose(false);
    }

    #endregion

    #region Resolvers

    /// <summary>
    /// Resolves the necessary pointers for cryptographic operations by loading the OQS library
    /// and initializing the required delegates for signing and verification operations.
    /// </summary>
    /// <remarks>
    /// This method ensures that the native library <c>oqs.dll</c> is loaded from the temporary location
    /// and all the required delegate functions for the Dilithium algorithm are resolved.
    /// If any error occurs during this process, a <see cref="CryptographyException"/> will be thrown.
    /// </remarks>
    private void DilithiumPointerResolve()
    {
        lock (_libLock)
        {
            try
            {
                if (!File.Exists(_dllPath))
                {
                    using var assemblyStream = Assembly.GetExecutingAssembly().GetManifestResourceStream(Immutable.Dilithium.ResourceName) ??
                        throw new CryptographyException($"{Immutable.Dilithium.ResourceName} is not found");

                    using (var stream = new FileStream(_dllPath, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.Read, 4096))
                        assemblyStream.CopyTo(stream);
                }

                _oqsLibraryHandle = NativeLibrary.Load(_dllPath);
                if (_oqsLibraryHandle == IntPtr.Zero)
                    throw new CryptographyException("Failed to load oqs.dll");

                _sig = _oqsSigNew(Immutable.Dilithium.AlgorithmName);
                if (_sig == IntPtr.Zero)
                    throw new CryptographyException("Failed to initialize Dilithium");

                ResolveDelegates();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error during initialization: " + ex.ToString());
                throw new CryptographyException(ex.Message);
            }
        }
    }

    /// <summary>
    /// Resolves all required delegate functions from the loaded OQS library.
    /// </summary>
    /// <remarks>
    /// This method sequentially resolves each required function pointer from the native library
    /// and assigns them to their respective delegate fields. If any function cannot be resolved,
    /// a <see cref="CryptographyException"/> will be thrown.
    /// </remarks>
    private void ResolveDelegates()
    {
        ResolveNewSignatureDelegate();
        ResolveSignatureFreeDelegate();
        ResolveGenerateKeyPairDelegate();
        ResolveSignDelegate();
        ResolveVerifyDelegate();
    }

    /// <summary>
    /// Resolves the OQS_SIG_new function from the native library and assigns it to the corresponding delegate.
    /// </summary>
    /// <remarks>
    /// This function is used to create new instances of signature schemes. The resolved delegate
    /// is stored in the <see cref="_oqsSigNew"/> field for later use.
    /// </remarks>
    private void ResolveNewSignatureDelegate()
    {
        Console.WriteLine("Resolving OQS_SIG_new...");
        _oqsSigNew = Marshal.GetDelegateForFunctionPointer<OQS_SIG_newDelegate>(
            NativeLibrary.GetExport(_oqsLibraryHandle, "OQS_SIG_new")
        );
    }

    /// <summary>
    /// Resolves the OQS_SIG_free function from the native library and assigns it to the corresponding delegate.
    /// </summary>
    /// <remarks>
    /// This function is used to free resources associated with a signature scheme instance. The resolved delegate
    /// is stored in the <see cref="_oqsSigFree"/> field for later use.
    /// </remarks>
    private void ResolveSignatureFreeDelegate()
    {
        Console.WriteLine("Resolving OQS_SIG_free...");
        _oqsSigFree = Marshal.GetDelegateForFunctionPointer<OQS_SIG_freeDelegate>(
            NativeLibrary.GetExport(_oqsLibraryHandle, "OQS_SIG_free")
        );
    }

    /// <summary>
    /// Resolves the OQS_SIG_keypair function from the native library and assigns it to the corresponding delegate.
    /// </summary>
    /// <remarks>
    /// This function is used to generate key pairs for the signature scheme. The resolved delegate
    /// is stored in the <see cref="_oqsSigKeypair"/> field for later use.
    /// </remarks>
    private void ResolveGenerateKeyPairDelegate()
    {
        Console.WriteLine("Resolving OQS_SIG_keypair...");
        _oqsSigKeypair = Marshal.GetDelegateForFunctionPointer<OQS_SIG_keypairDelegate>(
            NativeLibrary.GetExport(_oqsLibraryHandle, "OQS_SIG_keypair")
        );
    }

    /// <summary>
    /// Resolves the OQS_SIG_sign function from the native library and assigns it to the corresponding delegate.
    /// </summary>
    /// <remarks>
    /// This function is used to create signatures for messages. The resolved delegate
    /// is stored in the <see cref="_oqsSigSign"/> field for later use.
    /// </remarks>
    private void ResolveSignDelegate()
    {
        Console.WriteLine("Resolving OQS_SIG_sign...");
        _oqsSigSign = Marshal.GetDelegateForFunctionPointer<OQS_SIG_signDelegate>(
            NativeLibrary.GetExport(_oqsLibraryHandle, "OQS_SIG_sign")
        );
    }

    /// <summary>
    /// Resolves the OQS_SIG_verify function from the native library and assigns it to the corresponding delegate.
    /// </summary>
    /// <remarks>
    /// This function is used to verify message signatures. The resolved delegate
    /// is stored in the <see cref="_oqsSigVerify"/> field for later use.
    /// </remarks>
    private void ResolveVerifyDelegate()
    {
        Console.WriteLine("Resolving OQS_SIG_verify...");
        _oqsSigVerify = Marshal.GetDelegateForFunctionPointer<OQS_SIG_verifyDelegate>(
            NativeLibrary.GetExport(_oqsLibraryHandle, "OQS_SIG_verify")
        );
    }

    #endregion

    #region Dilitium 

    /// <summary>
    /// Generates a new public/private key pair using the Dilithium3 algorithm.
    /// </summary>
    /// <returns>A <see cref="KeyPairDetails"/> object containing the public and private keys as byte arrays.</returns>
    /// <exception cref="ObjectDisposedException">
    /// Thrown if the instance has been disposed.
    /// </exception>
    /// <remarks>
    /// The key generation is thread-safe and can be called concurrently from multiple threads.
    /// </remarks>
    public KeyPairDetails GenerateKeyPair()
    {
        ObjectDisposedException.ThrowIf(disposed, this);

        lock (_pointerLock)
        {
            byte[] publicKey = new byte[Immutable.Dilithium.PublicKeyLength];
            byte[] privateKey = new byte[Immutable.Dilithium.PrivateKeyLength];

            int result = _oqsSigKeypair(_sig, publicKey, privateKey);
            if (result != 0)
                throw new CryptographyException("An error occurred while attempting to generate a key pair");

            return new KeyPairDetails
            {
                PrivateKey = privateKey,
                PublicKey = publicKey
            };
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
        ObjectDisposedException.ThrowIf(disposed, this);

        lock (_pointerLock)
        {
            byte[] signature = new byte[Immutable.Dilithium.SignatureLength];

            int result = _oqsSigSign(_sig, signature, out long signatureLen, message, message.Length, privateKey);
            if (result != 0)
                throw new CryptographyException("An error occurred while attempting to sign a message");

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
        ObjectDisposedException.ThrowIf(disposed, this);

        lock (_pointerLock)
        {
            int result = _oqsSigVerify(_sig, message, message.Length, signature, signature.Length, publicKey);
            return result == 0;
        }
    }

    #endregion

    #region Disposing

    /// <summary>
    /// Releases the resources used by the <see cref="DilithiumSigner"/> instance.
    /// </summary>
    /// <param name="disposing">A boolean flag indicating whether the method is being called from the
    /// <see cref="Dispose"/> method (true) or from the finalizer (false).</param>
    /// <remarks>
    /// This method performs resource cleanup in two phases:
    /// <list type="bullet">
    /// <item>
    /// <description>If <c>disposing = true</c>, it releases both managed and unmanaged resources.</description>
    /// </item>
    /// <item>
    /// <description>If <c>disposing = false</c>, only unmanaged resources are released (called by the finalizer).</description>
    /// </item>
    /// </list>
    /// After disposal, the object should not be used again, and all related memory should be freed.
    /// </remarks>
    /// <exception cref="ObjectDisposedException">Thrown if the object is already disposed when this method is called.</exception>
    protected virtual void Dispose(bool disposing)
    {
        if (disposed)
            return;

        lock (_pointerLock)
        {
            if (_sig != IntPtr.Zero)
            {
                _oqsSigFree(_sig);
                _sig = IntPtr.Zero;
            }

            if (disposing)
            {
                if (_sig != IntPtr.Zero)
                {
                    NativeLibrary.Free(_oqsLibraryHandle);
                    _oqsLibraryHandle = IntPtr.Zero;
                }
            }
        }

        disposed = true;
    }

    /// <summary>
    /// Disposes the current <see cref="DilithiumSigner"/> instance, releasing all resources.
    /// </summary>
    /// <remarks>
    /// This method is called to explicitly release resources used by this object. It is recommended to call this
    /// method when the object is no longer needed to avoid resource leaks.
    /// </remarks>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    #endregion
}
