namespace Infrastructure.Data_Transfer_Object.Logger;

/// <summary>
/// Represents configuration arguments for logging behavior.
/// Determines what additional information should be included in log output.
/// </summary>
public class LoggerArgument
{
    /// <summary>
    /// Gets or sets a value indicating whether stack trace information should be included in logs.
    /// When true, exception logs will contain full stack trace details.
    /// </summary>
    public bool LogStackTrace { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether inner exceptions should be logged.
    /// When true, exception logs will recursively include all inner exceptions.
    /// </summary>
    public bool LogInnerExceptions { get; set; }
}
