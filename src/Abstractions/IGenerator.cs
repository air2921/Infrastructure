﻿using Infrastructure.Exceptions;

namespace Infrastructure.Abstractions;

/// <summary>
/// Interface for generating GUID combinations and random codes.
/// </summary>
public interface IRandomizer
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

    /// <summary>
    /// Generates a human-readable code with segments (e.g., "AB3X-9KJL").
    /// </summary>
    /// <param name="segmentLength">Length of each segment (default: 4).</param>
    /// <param name="segmentCount">Number of segments (default: 2).</param>
    /// <param name="separator">Separator character (default: '-').</param>
    /// <returns>A formatted random code.</returns>
    public string GenerateReadableCode(int segmentLength = 4, int segmentCount = 2, char separator = '-');

    /// <summary>
    /// Generates a random MAC address in standard format (XX:XX:XX:XX:XX:XX).
    /// </summary>
    /// <returns>Random MAC address string</returns>
    public string GenerateMacAddress();
}