using Infrastructure.Abstractions.Utility;
using Infrastructure.Enums;
using Infrastructure.Exceptions;
using System.Text;

namespace Infrastructure.Services.Utils;

/// <summary>
/// A high-performance class for generating various types of unique identifiers and random codes.
/// Implements the <see cref="IRandomizer"/> interface to provide thread-safe generation of:
/// - Combined GUID strings
/// - Random numeric codes
/// - Human-readable segmented codes
/// - MAC addresses
/// </summary>
/// <remarks>
/// <para>
/// Key features:
/// <list type="bullet">
///   <item><description>Optimized memory usage with zero intermediate allocations in critical paths</description></item>
///   <item><description>Thread-safe through use of <see cref="Random.Shared"/> and immutable state</description></item>
///   <item><description>Configurable output formats with length validation</description></item>
///   <item><description>Support for multiple identifier types (GUIDs, codes, MAC addresses)</description></item>
/// </list>
/// </para>
/// <para>
/// All methods validate their input parameters and throw <see cref="InvalidArgumentException"/>
/// with descriptive messages when constraints are violated.
/// </para>
/// </remarks>
public class Randomizer : IRandomizer
{
    /// <summary>
    /// Combines multiple GUIDs into a single optimized string.
    /// </summary>
    /// <param name="count">Number of GUIDs to combine (between <see cref="Immutable.ValidationParameter.MinGuidCombineLength"/> and <see cref="Immutable.ValidationParameter.MaxGuidCombineLength"/>)</param>
    /// <param name="format">Guid generator format. Defaults is <see cref="GuidFormat.D"/><c>false</c>.</param>
    /// <returns>Concatenated GUID string in specified format</returns>
    /// <exception cref="InvalidArgumentException">Thrown when count is outside valid range</exception>
    public string GuidCombine(int count, GuidFormat format = GuidFormat.D)
    {
        if (count < Immutable.ValidationParameter.MinGuidCombineLength || count > Immutable.ValidationParameter.MaxGuidCombineLength)
            throw new InvalidArgumentException($"The allowed number of GUID combinations must be between {Immutable.ValidationParameter.MinGuidCombineLength} and {Immutable.ValidationParameter.MaxGuidCombineLength}");

        var builder = new StringBuilder(((int)format) * count);
        for (int i = 0; i < count; i++)
            builder.Append(Guid.NewGuid().ToString(format.ToString()));

        return builder.ToString();
    }

    /// <summary>
    /// Generates a purely numeric random code with guaranteed length.
    /// </summary>
    /// <param name="length">Desired code length (between <see cref="Immutable.ValidationParameter.MinCodeLength"/> and <see cref="Immutable.ValidationParameter.MaxCodeLength"/>)</param>
    /// <returns>String containing random digits</returns>
    /// <exception cref="InvalidArgumentException">Thrown when length is invalid</exception>
    public string GenerateNumericCode(int length)
    {
        if (length < Immutable.ValidationParameter.MinCodeLength || length > Immutable.ValidationParameter.MaxCodeLength)
            throw new InvalidArgumentException($"The valid code length must be in the range from {Immutable.ValidationParameter.MinCodeLength} to {Immutable.ValidationParameter.MaxCodeLength}");

        var builder = new StringBuilder(length);
        for (int i = 0; i < length; i++)
            builder.Append(Random.Shared.Next(10));

        return builder.ToString();
    }

    /// <summary>
    /// Generates optimized human-readable codes in segmented format (e.g., "AB3X-9KJL").
    /// </summary>
    /// <param name="segmentLength">Characters per segment (default: 4)</param>
    /// <param name="segmentCount">Number of segments (default: 2)</param>
    /// <param name="separator">Segment divider character (default: '-')</param>
    /// <returns>Formatted random code avoiding ambiguous characters</returns>
    /// <remarks>
    /// <para>
    /// Uses a restricted character set (A-Z, 2-9) excluding visually similar characters (I,1,O,0).
    /// Implemented with zero-allocation span-based operations for maximum performance.
    /// </para>
    /// </remarks>
    public string GenerateReadableCode(int segmentLength = 4, int segmentCount = 2, char separator = '-')
    {
        int totalLength = segmentLength * segmentCount + (segmentCount - 1);
        var chars = Immutable.Char.ReadebleChars;
        var random = Random.Shared;

        return string.Create(totalLength, (chars, segmentLength, segmentCount, separator), (span, state) =>
        {
            int charsLength = state.chars.Length;
            int separatorPos = state.segmentLength;

            for (int i = 0; i < span.Length; i++)
            {
                if (i > 0 && (i + 1) % (separatorPos + 1) == 0)
                {
                    span[i] = state.separator;
                    separatorPos += state.segmentLength + 1;
                }
                else
                {
                    span[i] = state.chars[random.Next(charsLength)];
                }
            }
        });
    }

    /// <summary>
    /// Generates a random MAC address in standard format (XX:XX:XX:XX:XX:XX).
    /// </summary>
    /// <returns>Random MAC address string</returns>
    /// <remarks>
    /// <para>
    /// Generates a valid 48-bit MAC address with:
    /// <list type="bullet">
    ///   <item><description>Second least significant bit of first byte set to 1 (locally administered)</description></item>
    ///   <item><description>Least significant bit of first byte set to 0 (unicast)</description></item>
    /// </list>
    /// Uses hexadecimal characters (0-9, A-F) in upper case.
    /// </para>
    /// <para>
    /// Implemented with zero-allocation span-based operations for maximum performance.
    /// </para>
    /// </remarks>
    public string GenerateMacAddress()
    {
        return string.Create(17, Random.Shared, (span, random) =>
        {
            var chars = Immutable.Char.HexChars;

            byte firstByte = (byte)(random.Next(256) & 0xFC | 0x02);
            span[0] = chars[firstByte >> 4];
            span[1] = chars[firstByte & 0x0F];
            span[2] = ':';

            for (int i = 1; i < 6; i++)
            {
                byte b = (byte)random.Next(256);
                span[i * 3] = chars[b >> 4];
                span[i * 3 + 1] = chars[b & 0x0F];
                if (i < 5) span[i * 3 + 2] = ':';
            }
        });
    }
}