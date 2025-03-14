using Infrastructure.Abstractions;
using Infrastructure.Data_Transfer_Object.Authorization;
using Infrastructure.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Infrastructure.Services.Authorization;

/// <summary>
/// A class responsible for publishing JSON Web Tokens (JWT) based on provided details.
/// This class implements the <see cref="IPublisher{JsonWebTokenDetails}"/> interface to generate JWTs.
/// </summary>
/// <param name="authorizationOptions">Configuration options for JWT generation, such as issuer, audience, key, and expiration.</param>
/// <remarks>
/// This class uses the provided <see cref="AuthorizationConfigureOptions"/> to configure the JWT, including signing credentials, claims, and expiration.
/// </remarks>
public class JsonWebTokenPublisher(AuthorizationConfigureOptions authorizationOptions) : IPublisher<JsonWebTokenDetails>
{
    /// <summary>
    /// Generates a JSON Web Token (JWT) based on the provided details.
    /// </summary>
    /// <param name="details">The details required to generate the JWT, including user ID, role, and additional claims.</param>
    /// <returns>A signed JWT as a string.</returns>
    /// <example>
    /// <code>
    /// var options = new AuthorizationConfigureOptions
    /// {
    ///     Issuer = "MyApp",
    ///     Audience = "MyAppClient",
    ///     Key = "my-secret-key",
    ///     Expiration = TimeSpan.FromHours(1)
    /// };
    /// var publisher = new JsonWebTokenPublisher(options);
    /// var jwtDetails = new JsonWebTokenDetails
    /// {
    ///     UserId = "12345",
    ///     Role = "Admin",
    ///     Claims = new List<Claim> { new Claim("Permission", "Read") }
    /// };
    /// var token = publisher.Publish(jwtDetails);
    /// </code>
    /// </example>
    public string Publish(JsonWebTokenDetails details)
    {
        var key = new SymmetricSecurityKey(authorizationOptions.Encoding.GetBytes(authorizationOptions.Key));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, details.UserId),
            new(ClaimTypes.Role, details.Role)
        };
        claims.AddRange(details.Claims);

        var token = new JwtSecurityToken(
            issuer: authorizationOptions.Issuer,
            audience: authorizationOptions.Audience,
            claims: claims,
            expires: DateTime.UtcNow + authorizationOptions.Expiration,
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
