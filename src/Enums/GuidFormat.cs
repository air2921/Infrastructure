namespace Infrastructure.Enums;

/// <summary>
/// Enumeration of supported GUID string formats.
/// <b>Format "D" (36 characters with hyphens) is the most commonly used.</b>
/// </summary>
public enum GuidFormat
{
    /// <summary>
    /// 32-digit hexadecimal number without hyphens (N).
    /// Example: "00000000000000000000000000000000"
    /// </summary>
    N = 32,

    /// <summary>
    /// 36-character hexadecimal number with hyphens (D).
    /// Example: "00000000-0000-0000-0000-000000000000"
    /// <b>This is the most common and readable format.</b>
    /// </summary>
    D = 36,

    /// <summary>
    /// 38-character hexadecimal number with hyphens and enclosing braces (P).
    /// Example: "{00000000-0000-0000-0000-000000000000}"
    /// </summary>
    P = 38,

    /// <summary>
    /// 68-character hexadecimal number with hyphens and enclosing parentheses (X).
    /// Example: "(00000000-0000-0000-0000-000000000000)"
    /// Typically used in special cases like COM programming.
    /// </summary>
    X = 68
}
