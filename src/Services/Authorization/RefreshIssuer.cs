using Infrastructure.Abstractions;
using Infrastructure.Data_Transfer_Object.Authorization;

namespace Infrastructure.Services.Authorization;

/// <remarks>
/// This class uses the provided <see cref="IGenerator"/> to create a refresh token by combining a specified number of GUIDs.
/// The number of GUIDs to combine is determined by the <see cref="RefreshDetails.CombineCount"/> property.
/// </remarks>
public class RefreshIssuer(IRandomizer randomizer) : ITokenIssuer<RefreshDetails>
{
    /// <summary>
    /// Generates a refresh token by combining multiple GUIDs.
    /// </summary>
    /// <param name="details">The details required to generate the refresh token, including the number of GUIDs to combine.</param>
    /// <returns>A refresh token as a string, created by combining the specified number of GUIDs.</returns>
    public string Release(RefreshDetails details)
        => randomizer.GuidCombine(details.CombineCount);
}
