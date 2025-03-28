using Infrastructure.Abstractions.Utility;
using Infrastructure.Data_Transfer_Object.Authorization;
using Infrastructure.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Infrastructure.Services.Authorization;

/// <summary>
/// A class responsible for publishing JSON Web Tokens (JWT) based on provided details.
/// This class implements the <see cref="ITokenIssuer{JsonWebTokenDetails}"/> interface to generate JWTs.
/// </summary>
/// <param name="authorizationOptions">Configuration options for JWT generation, such as issuer, audience, key, and expiration.</param>
/// <remarks>
/// This class uses the provided <see cref="AuthorizationConfigureOptions"/> to configure the JWT, including signing credentials, claims, and expiration.
/// </remarks>
public class JsonWebTokenIssuer(AuthorizationConfigureOptions authorizationOptions) : ITokenIssuer<JsonWebTokenDetails>
{
    /// <summary>
    /// Generates a JSON Web Token (JWT) based on the provided details.
    /// </summary>
    /// <param name="details">The details required to generate the JWT, including user ID, role, and additional claims.</param>
    /// <returns>A signed JWT as a string.</returns>
    public string Release(JsonWebTokenDetails details)
    {
        var key = new SymmetricSecurityKey(authorizationOptions.Encoding.GetBytes(authorizationOptions.Key));
        var credentials = new SigningCredentials(key, authorizationOptions.Algorithm);

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
