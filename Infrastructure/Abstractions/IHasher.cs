using BCrypt.Net;
namespace Infrastructure.Abstractions;

public interface IHasher
{
    public string Hash(string password, HashType hashType = HashType.SHA512);
    public bool Verify(string input, string src, HashType hashType = HashType.SHA512);
}
