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
    [JsonPropertyName("nameOf")]
    public required string OuterExceptionName { get; set; }

    /// <summary>
    /// Gets or sets the dictionary of inner exceptions with keys formatted as "1-InvalidCastExcepion and values as message of current inner exception".
    /// </summary>
    [JsonPropertyName("innerExceptions")]
    public required Dictionary<string, string> InnerExceptions { get; set; } = [];

    /// <summary>
    /// Gets or sets the exception message.
    /// </summary>
    [JsonPropertyName("message")]
    public required string? Message { get; set; }

    /// <summary>
    /// Gets or sets the stack trace of the exception.
    /// </summary>
    [JsonPropertyName("stackTrace")]
    public required string? StackTrace { get; set; }
}
