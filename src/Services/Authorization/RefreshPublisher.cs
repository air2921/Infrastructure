using Infrastructure.Abstractions;
using Infrastructure.Data_Transfer_Object.Authorization;

namespace Infrastructure.Services.Authorization;

/// <remarks>
/// This class uses the provided <see cref="IGenerator"/> to create a refresh token by combining a specified number of GUIDs.
/// The number of GUIDs to combine is determined by the <see cref="RefreshDetails.CombineCount"/> property.
/// </remarks>
public class RefreshPublisher(IGenerator generator) : IPublisher<RefreshDetails>
{
    /// <summary>
    /// Generates a refresh token by combining multiple GUIDs.
    /// </summary>
    /// <param name="details">The details required to generate the refresh token, including the number of GUIDs to combine.</param>
    /// <returns>A refresh token as a string, created by combining the specified number of GUIDs.</returns>
    /// <example>
    /// <code>
    /// var generator = new GuidGenerator(); // Assuming GuidGenerator implements IGenerator
    /// var publisher = new RefreshPublisher(generator);
    /// var refreshDetails = new RefreshDetails { CombineCount = 3 };
    /// var refreshToken = publisher.Publish(refreshDetails);
    /// // refreshToken will be a string combining 3 GUIDs.
    /// </code>
    /// </example>
    public string Publish(RefreshDetails details)
        => generator.GuidCombine(details.CombineCount);
}
