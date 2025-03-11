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

    private bool _disposed = false;

    static DilithiumSigner()
    {
        try
        {
            Console.WriteLine("Loading oqs.dll from resources...");

            string resourceName = "Infrastructure.Assembly.oqs.dll";

            using var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName) ??
                throw new CryptographyException("DLL not found in resources");

            string tempFilePath = Path.Combine(Path.GetTempPath(), "oqs.dll");
            using var fileStream = new FileStream(tempFilePath, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None, 4096, true);
            stream.CopyTo(fileStream);

            _oqsLibraryHandle = NativeLibrary.Load(tempFilePath);
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
        _sig = _oqsSigNew("Dilithium3");
        if (_sig == IntPtr.Zero)
            throw new CryptographyException("Failed to initialize Dilithium");
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
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

            _disposed = true;
        }
    }

    ~DilithiumSigner()
    {
        Dispose(false);
    }

    public (byte[] publicKey, byte[] privateKey) GenerateKeyPair()
    {
        if (_disposed)
            throw new ObjectDisposedException("DilithiumSignature");

        byte[] publicKey = new byte[OQS_SIG_length_public_key()];
        byte[] privateKey = new byte[OQS_SIG_length_private_key()];

        int result = _oqsSigKeypair(_sig, publicKey, privateKey);
        if (result != 0)
            throw new CryptographyException("Failed to generate key pair");

        return (publicKey, privateKey);
    }

    public byte[] Sign(byte[] message, byte[] privateKey)
    {
        if (_disposed)
            throw new ObjectDisposedException("DilithiumSignature");

        byte[] signature = new byte[OQS_SIG_length_signature()];

        int result = _oqsSigSign(_sig, signature, out long signatureLen, message, message.Length, privateKey);
        if (result != 0)
            throw new CryptographyException("Failed to sign message");

        return signature;
    }

    public bool Verify(byte[] message, byte[] signature, byte[] publicKey)
    {
        if (_disposed)
            throw new ObjectDisposedException("DilithiumSignature");

        int result = _oqsSigVerify(_sig, message, message.Length, signature, signature.Length, publicKey);
        return result == 0;
    }

    private int OQS_SIG_length_public_key()
        => 1952;

    private int OQS_SIG_length_private_key()
        => 4000;

    private int OQS_SIG_length_signature()
        => 3293;
}
