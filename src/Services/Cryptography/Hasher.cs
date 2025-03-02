using BCrypt.Net;
using Infrastructure.Abstractions;
using Infrastructure.Exceptions;
using Microsoft.Extensions.Logging;


namespace Infrastructure.Services.Cryptography;

public class Hasher(ILogger<Hasher> logger) : IHasher
{
    public string Hash(string password, HashType hashType = HashType.SHA512)
    {
        try
        {
            return BCrypt.Net.BCrypt.EnhancedHashPassword(password, hashType);
        }
        catch (Exception ex)
        {
            logger.LogError(ex.ToString());
            throw new CryptographyException();
        }
    }


    public bool Verify(string input, string src, HashType hashType = HashType.SHA512)
    {
        try
        {
            return BCrypt.Net.BCrypt.EnhancedVerify(input, src, hashType);
        }
        catch (Exception ex)
        {
            logger.LogError(ex.ToString());
            throw new CryptographyException();
        }
    }
}
