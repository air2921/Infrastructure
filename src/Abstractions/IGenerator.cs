using Infrastructure.Exceptions;

namespace Infrastructure.Abstractions;

/// <summary>
/// Interface for generating GUID combinations and random numeric codes.
/// </summary>
public interface IGenerator
{
    /// <summary>
    /// Combines multiple GUIDs into a single string.
    /// </summary>
    /// <param name="count">The number of GUIDs to combine. The value must be between <see cref="Immutable.MinGuidCombineLength"/> and <see cref="Immutable.MaxGuidCombineLength"/>.</param>
    /// <param name="useNoHyphensFormat">Indicates whether to format the GUIDs without hyphens. Defaults to <c>false</c>.</param>
    /// <returns>A concatenated string of GUIDs.</returns>
    /// <exception cref="InvalidArgumentException">Thrown if the <paramref name="count"/> is outside the supported range, defined in <see cref="Immutable.MinGuidCombineLength"/> and <see cref="Immutable.MaxGuidCombineLength"/>.</exception>
    public string GuidCombine(int count, bool useNoHyphensFormat = false);

    /// <summary>
    /// Generates a random numeric code of the specified length.
    /// </summary>
    /// <param name="length">The length of the numeric code to generate. The value must be between <see cref="Immutable.MinCodeLength"/> and <see cref="Immutable.MaxCodeLength"/>.</param>
    /// <returns>A random numeric code as a string.</returns>
    /// <exception cref="InvalidArgumentException">Thrown if the <paramref name="length"/> is outside the supported range, defined in <see cref="Immutable.MinCodeLength"/> and <see cref="Immutable.MaxCodeLength"/>.</exception>
    public string GenerateCode(int length);
}