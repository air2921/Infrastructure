using Infrastructure.Data_Transfer_Object.Authorization;

namespace Infrastructure.Abstractions;

/// <summary>
/// Represents a publisher for generating authorization tokens, such as JWT and refresh tokens.
/// </summary>
/// <typeparam name="TAuthorization">The type of authorization details, which must inherit from <see cref="AuthorizationDetails"/>.</typeparam>
public interface ITokenIssuer<TAuthorization> where TAuthorization : AuthorizationDetails
{
    /// <summary>
    /// Generates an authorization token (e.g., JWT) based on the provided authorization details.
    /// </summary>
    /// <param name="details">The authorization details used to generate the token.</param>
    /// <returns>A string representing the generated authorization token (e.g., JWT).</returns>
    public string Release(TAuthorization details);
}