using Infrastructure.Abstractions;
using Infrastructure.Exceptions;
using System.Text;

namespace Infrastructure.Services.Utils;

/// <summary>
/// A class responsible for generating various types of identifiers and codes, such as combined GUIDs and random numeric codes.
/// This class implements the <see cref="IGenerator"/> interface and provides functionality to generate unique identifiers 
/// and random codes with configurable lengths and formats.
/// </summary>
/// <remarks>
/// This class provides two main functionalities:
/// 1. Combining multiple GUIDs into a single string, either in a standard format with hyphens or a condensed format without hyphens.
/// 2. Generating a random numeric code of a specified length.
/// Both functionalities have constraints on the length, and exceptions are thrown if invalid values are provided.
/// </remarks>
public class Generator : IGenerator
{
    private static readonly Random _rnd = new();

    private static readonly Lazy<InvalidAgrumentException> _guidCombineError = new(() => new($"The allowed number of GUID combinations must be between {Immutable.MinGuidCombineLength} and {Immutable.MaxGuidCombineLength}"), LazyThreadSafetyMode.ExecutionAndPublication);
    private static readonly Lazy<InvalidAgrumentException> _codeCombineError = new(() => new($"The valid code length must be in the range from {Immutable.MinCodeLength} to {Immutable.MaxCodeLength}"), LazyThreadSafetyMode.ExecutionAndPublication);

    /// <summary>
    /// Combines multiple GUIDs into a single string.
    /// </summary>
    /// <param name="count">The number of GUIDs to combine.</param>
    /// <param name="useNoHyphensFormat">Indicates whether to format the GUIDs without hyphens. Defaults to <c>false</c>.</param>
    /// <returns>A concatenated string of GUIDs.</returns>
    /// <exception cref="InvalidAgrumentException">Thrown if the count of GUIDs is outside the supported range defined in <see cref="Immutable.MinGuidCombineLength"/> and <see cref="Immutable.MaxGuidCombineLength"/>.</exception>
    /// <example>
    /// <code>
    /// var generator = new Generator();
    /// string combinedGuids = generator.GuidCombine(3, true);
    /// </code>
    /// </example>
    public string GuidCombine(int count, bool useNoHyphensFormat = false)
    {
        if (count < Immutable.MinGuidCombineLength || count >= Immutable.MaxGuidCombineLength)
            throw _guidCombineError.Value;

        var builder = new StringBuilder(useNoHyphensFormat ? 32 : 36 * count);
        for (int i = 0; i < count; i++)
            builder.Append(Guid.NewGuid().ToString(useNoHyphensFormat ? "N" : string.Empty));

        return builder.ToString();
    }

    /// <summary>
    /// Generates a random numeric code of the specified length.
    /// </summary>
    /// <param name="length">The length of the numeric code to generate.</param>
    /// <returns>A random numeric code as an string.</returns>
    /// <exception cref="InvalidAgrumentException">Thrown if the length is outside the supported range defined in <see cref="Immutable.MinCodeLength"/> and <see cref="Immutable.MaxCodeLength"/>.</exception>
    /// <example>
    /// <code>
    /// var generator = new Generator();
    /// int code = generator.GenerateCode(6);
    /// </code>
    /// </example>
    public string GenerateCode(int length)
    {
        if (length < Immutable.MinCodeLength || length >= Immutable.MaxCodeLength)
            throw _codeCombineError.Value;

        var builder = new StringBuilder(length);
        for (int i = 0; i < length; i++)
            builder.Append(_rnd.Next(10));

        return builder.ToString();
    }
}
