using Infrastructure.Abstractions;
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
    public static IInfrastructureBuilder AddCryptography(this IInfrastructureBuilder builder)
    {
        builder.Services.AddScoped<IHasher, Hasher>();
        builder.Services.AddSingleton<ISigner, DilithiumSigner>();

        return builder;
    }
}
