using System.Text.Json.Serialization;

namespace Infrastructure.Data_Transfer_Object.Logger;

/// <summary>
/// Represents a complete structured log entry with all relevant details.
/// </summary>
/// <remarks>
/// This class is serialized to JSON when logging messages. The structure includes
/// timestamp, message content, optional exception details, and additional parameters.
/// </remarks>
public class LogDetails
{
    /// <summary>
    /// Gets or sets the UTC timestamp when the log was created.
    /// </summary>
    /// <value>Initialized to <see cref="DateTimeOffset.UtcNow"/> when created.</value>
    [JsonPropertyName("loggedAt")]
    public DateTimeOffset LoggedAt { get; set; } = DateTimeOffset.UtcNow;

    /// <summary>
    /// Gets or sets the main log message content.
    /// </summary>
    /// <example>"Failed to process user request"</example>
    [JsonPropertyName("message")]
    public required string Message { get; set; }

    /// <summary>
    /// Gets or sets the exception details, if any.
    /// </summary>
    /// <remarks>
    /// This property will be omitted from JSON output when null or default.
    /// </remarks>
    [JsonPropertyName("exception")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public required ExceptionDetails? Exception { get; set; }

    /// <summary>
    /// Gets or sets additional parameters for the log message.
    /// </summary>
    /// <value>Initialized to an empty collection when created.</value>
    [JsonPropertyName("args")]
    public required IEnumerable<string> Params { get; set; } = [];
}
