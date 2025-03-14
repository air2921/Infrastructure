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
    /// <example>
    /// <code>
    /// var jwtDetails = new JsonWebTokenDetails { UserId = "12345", Role = "Admin" };
    /// </code>
    /// </example>
    public required string UserId { get; set; }

    /// <summary>
    /// Gets or sets the role of the user associated with the JWT.
    /// </summary>
    /// <example>
    /// <code>
    /// var jwtDetails = new JsonWebTokenDetails { UserId = "12345", Role = "Admin" };
    /// </code>
    /// </example>
    public required string Role { get; set; }

    /// <summary>
    /// Gets or sets a collection of claims associated with the JWT.
    /// </summary>
    /// <example>
    /// <code>
    /// var jwtDetails = new JsonWebTokenDetails
    /// {
    ///     UserId = "12345",
    ///     Role = "Admin",
    ///     Claims = new List<Claim> { new Claim("Permission", "Read") }
    /// };
    /// </code>
    /// </example>
    public IEnumerable<Claim> Claims { get; set; } = [];
}
