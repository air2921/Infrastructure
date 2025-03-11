using Infrastructure.Abstractions;
using Infrastructure.Exceptions;
using System.Reflection;
using System.Runtime.InteropServices;

namespace Infrastructure.Services.Cryptography;

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

    static DilithiumSigner()
    {
        try
        {
            Console.WriteLine($"Loading {ResourceName}...");

            using var assemblyStream = Assembly.GetExecutingAssembly().GetManifestResourceStream(ResourceName) ??
                throw new CryptographyException("DLL not found in resources");

            using var fileStream = new FileStream(_dllPath, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None, 4096, true);
            assemblyStream.CopyTo(fileStream);

            _oqsLibraryHandle = NativeLibrary.Load(_dllPath);
            if (_oqsLibraryHandle == IntPtr.Zero)
                throw new CryptographyException("Failed to load oqs.dll");

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

    public DilithiumSigner()
    {
        // Инициализация алгоритма
        _sig = _oqsSigNew(AlgorithmName);
        if (_sig == IntPtr.Zero)
            throw new CryptographyException("Failed to initialize Dilithium");
    }

    ~DilithiumSigner()
    {
        Dispose(false);
    }

    public (byte[] publicKey, byte[] privateKey) GenerateKeyPair()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        lock (_lock)
        {
            byte[] publicKey = new byte[PublicKeyLength];
            byte[] privateKey = new byte[PrivateKeyLength];

            int result = _oqsSigKeypair(_sig, publicKey, privateKey);
            if (result != 0)
                throw new CryptographyException("Failed to generate key pair");

            return (publicKey, privateKey);
        }
    }

    public byte[] Sign(byte[] message, byte[] privateKey)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        lock (_lock)
        {
            byte[] signature = new byte[SignatureLength];

            int result = _oqsSigSign(_sig, signature, out long signatureLen, message, message.Length, privateKey);
            if (result != 0)
                throw new CryptographyException("Failed to sign message");

            return signature;
        }
    }

    public bool Verify(byte[] message, byte[] signature, byte[] publicKey)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        lock (_lock)
        {
            int result = _oqsSigVerify(_sig, message, message.Length, signature, signature.Length, publicKey);
            return result == 0;
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

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
