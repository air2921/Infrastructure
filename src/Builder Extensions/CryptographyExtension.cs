using Infrastructure.Abstractions.Cryptography;
using Infrastructure.Configuration;
using Infrastructure.Services.Cryptography;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.Builder_Extensions;

/// <summary>
/// Provides extension methods for adding cryptography services to an <see cref="IInfrastructureBuilder"/>.
/// </summary>
public static class CryptographyExtension
{
    /// <summary>
    /// Adds cryptography services to the <see cref="IInfrastructureBuilder"/>.
    /// </summary>
    /// <param name="builder">The infrastructure builder to which the cryptography services will be added.</param>
    /// <returns>The updated <see cref="IInfrastructureBuilder"/> with the added cryptography services.</returns>
    /// <remarks>
    /// This method registers the following services for Dependency Injection (DI):
    /// <list type="bullet">
    ///     <item><description><see cref="IHasher"/> - Scoped service for hashing data.</description></item>
    ///     <item><description><see cref="ICipher"/> - Scoped service for encrypt and decrypt data.</description></item>
    /// </list>
    /// </remarks>
    public static IInfrastructureBuilder AddCryptography(this IInfrastructureBuilder builder)
    {
        builder.Services.AddScoped<IHasher, Hasher>();
        builder.Services.AddScoped<ICipher, AesCipher>();

        return builder;
    }
}
