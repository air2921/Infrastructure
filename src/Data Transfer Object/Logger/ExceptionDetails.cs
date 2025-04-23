using System.Text.Json.Serialization;

namespace Infrastructure.Data_Transfer_Object.Logger;

/// <summary>
/// Represents detailed information about an exception for structured logging.
/// </summary>
/// <remarks>
/// This class is used to serialize exception information into a structured JSON format
/// when logging errors. All properties are marked as required to ensure complete
/// exception information when present.
/// </remarks>
public class ExceptionDetails
{
    /// <summary>
    /// Gets or sets the type name of the exception.
    /// </summary>
    /// <example>"NullReferenceException"</example>
    [JsonPropertyName("nameOf")]
    public required string Type { get; set; }

    /// <summary>
    /// Gets or sets the exception message.
    /// </summary>
    /// <example>"Object reference not set to an instance of an object."</example>
    [JsonPropertyName("message")]
    public required string? Message { get; set; }

    /// <summary>
    /// Gets or sets the stack trace of the exception.
    /// </summary>
    /// <example>
    /// "at Program.Main() in C:\Project\Program.cs:line 10"
    /// </example>
    [JsonPropertyName("stackTrace")]
    public required string? StackTrace { get; set; }
}
