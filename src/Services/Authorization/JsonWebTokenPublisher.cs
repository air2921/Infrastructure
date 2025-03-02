using Infrastructure.Abstractions;
using Infrastructure.Data_Transfer_Object.Authorization;
using Infrastructure.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Infrastructure.Services.Authorization;

public class JsonWebTokenPublisher(AuthorizationConfigureOptions authorizationOptions) : IPublisher<JsonWebTokenDetails>
{
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
