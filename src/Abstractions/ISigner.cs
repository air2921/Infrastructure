namespace Infrastructure.Abstractions;

public interface ISigner
{
    public (byte[] publicKey, byte[] privateKey) GenerateKeyPair();
    public byte[] Sign(byte[] message, byte[] privateKey);
    public bool Verify(byte[] message, byte[] signature, byte[] publicKey);
}
