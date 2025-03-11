using Infrastructure.Abstractions;
using Infrastructure.Configuration;
using Infrastructure.Services.Cryptography;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.Builder_Extensions;

public static class CryptographyExtension
{
    public static IInfrastructureBuilder AddCryptography(this IInfrastructureBuilder builder)
    {
        builder.Services.AddScoped<IHasher, Hasher>();
        builder.Services.AddSingleton<ISigner, DilithiumSigner>();

        return builder;
    }
}
