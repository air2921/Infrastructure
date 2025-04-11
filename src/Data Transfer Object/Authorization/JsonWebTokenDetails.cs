using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

namespace Infrastructure.Data_Transfer_Object.Authorization;

/// <summary>
/// Represents details of a JSON Web Token (JWT) used for authorization.
/// This class includes information such as the user ID, role, and claims associated with the token.
/// </summary>
/// <remarks>
/// This class is used to encapsulate JWT-specific details for authorization purposes.
/// </remarks>
public class JsonWebTokenDetails : AuthorizationDetails
{
    /// <summary>
    /// Gets or sets the unique identifier of the user associated with the JWT.
    /// </summary>
    [Required]
    public string UserId { get; set; } = null!;

    /// <summary>
    /// Gets or sets the role of the user associated with the JWT.
    /// </summary>
    [Required]
    public string Role { get; set; } = null!;

    /// <summary>
    /// Gets or sets a collection of claims associated with the JWT.
    /// </summary>
    public IEnumerable<Claim> Claims { get; set; } = [];
}
