using System.Security.Claims;

namespace Infrastructure.Data_Transfer_Object.Authorization;

public class JwtDetails : AuthorizationDetails
{
    public required string UserId { get; set; }
    public required string Role { get; set; }
    public IEnumerable<Claim> Claims { get; set; } = [];
}
