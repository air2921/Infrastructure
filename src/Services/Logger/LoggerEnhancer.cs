using Infrastructure.Abstractions.External_Services;
using Infrastructure.Data_Transfer_Object.Logger;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace Infrastructure.Services.Logger;

/// <summary>
/// A logger enhancer that provides extended logging capabilities for a specific category type.
/// This class wraps an <see cref="ILogger{TCategory}"/> instance and adds structured logging functionality.
/// </summary>
/// <typeparam name="TCategory">The type whose name is used for the logger category name.</typeparam>
/// <param name="logger">The underlying logger instance to enhance.</param>
public class LoggerEnhancer<TCategory>(ILogger<TCategory> logger) : ILoggerEnhancer<TCategory>
{
    /// <summary>
    /// Logs an informational message with structured parameters.
    /// </summary>
    /// <param name="message">The message template string.</param>
    /// <param name="args">Additional parameters to include in the structured log.</param>
    public void LogInformation(string message, params string[] args)
    {
        var log = FormatLogMessage(null, message, args);
#pragma warning disable CA2254
        logger.LogInformation(log);
#pragma warning restore CA2254
    }

    /// <summary>
    /// Logs a warning message with structured parameters.
    /// </summary>
    /// <param name="message">The message template string.</param>
    /// <param name="args">Additional parameters to include in the structured log.</param>
    public void LogWarning(string message, params string[] args)
    {
        var log = FormatLogMessage(null, message, args);
#pragma warning disable CA2254
        logger.LogWarning(log);
#pragma warning restore CA2254
    }

    /// <summary>
    /// Logs a critical error message with structured parameters.
    /// </summary>
    /// <param name="message">The message template string.</param>
    /// <param name="args">Additional parameters to include in the structured log.</param>
    public void LogCritical(string message, params string[] args)
    {
        var log = FormatLogMessage(null, message, args);
#pragma warning disable CA2254
        logger.LogCritical(log);
#pragma warning restore CA2254
    }

    /// <summary>
    /// Logs an error message with structured parameters.
    /// </summary>
    /// <param name="message">The message template string.</param>
    /// <param name="args">Additional parameters to include in the structured log.</param>
    public void LogError(string message, params string[] args)
    {
        var log = FormatLogMessage(null, message, args);
#pragma warning disable CA2254
        logger.LogError(log);
#pragma warning restore CA2254
    }

    /// <summary>
    /// Logs an error message with an exception and structured parameters.
    /// </summary>
    /// <param name="exception">The exception to log.</param>
    /// <param name="message">The message template string.</param>
    /// <param name="args">Additional parameters to include in the structured log.</param>
    public void LogError(Exception? exception, string message, params string[] args)
    {
        var log = FormatLogMessage(exception, message, args);
#pragma warning disable CA2254
        logger.LogError(log);
#pragma warning restore CA2254
    }

    /// <summary>
    /// Formats the log message along with exception details (if provided) and additional parameters
    /// into a structured JSON string.
    /// </summary>
    /// <param name="exception">The exception to include in the log, or null.</param>
    /// <param name="message">The message template string.</param>
    /// <param name="args">Additional parameters to include in the structured log.</param>
    /// <returns>A JSON string containing all log details in a structured format.</returns>
    private static string FormatLogMessage(Exception? exception, string message, IEnumerable<string> args)
    {
        ExceptionDetails? exceptionDetails = null;

        if (exception is not null)
        {
            exceptionDetails = new()
            {
                Type = exception.GetType().Name,
                Message = exception.Message,
                StackTrace = exception.StackTrace
            };
        }

        var log = new LogDetails()
        {
            Exception = exceptionDetails,
            Params = args,
            Message = message,
        };

        return JsonSerializer.Serialize(log);
    }
}
