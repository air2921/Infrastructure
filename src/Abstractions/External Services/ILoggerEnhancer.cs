using Infrastructure.Data_Transfer_Object.Logger;
using Infrastructure.Services.Logger;

namespace Infrastructure.Abstractions.External_Services;

/// <summary>
/// Provides enhanced logging capabilities for a specific category type with structured logging support.
/// </summary>
/// <typeparam name="TCategory">The covariant type whose name is used for the logger category name.</typeparam>
/// <remarks>
/// This interface defines enhanced logging methods that support structured logging by automatically
/// serializing messages and parameters into JSON format. It's designed to work with the <see cref="LoggerEnhancer{TCategory}"/>
/// implementation.
/// </remarks>
public interface ILoggerEnhancer<out TCategory>
{
    /// <summary>
    /// Logs an informational message with additional structured parameters.
    /// </summary>
    /// <param name="message">The descriptive message to log.</param>
    /// <param name="args">Additional parameters to include in the structured log entry.</param>
    public void LogInformation(string message, params object?[] args);

    /// <summary>
    /// Logs a warning message with additional structured parameters.
    /// </summary>
    /// <param name="message">The descriptive message to log.</param>
    /// <param name="args">Additional parameters to include in the structured log entry.</param>
    public void LogWarning(string message, params object?[] args);

    /// <summary>
    /// Logs a critical error message with additional structured parameters.
    /// </summary>
    /// <param name="message">The descriptive message to log.</param>
    /// <param name="args">Additional parameters to include in the structured log entry.</param>
    public void LogCritical(string message, params object?[] args);

    /// <summary>
    /// Logs an error message with additional structured parameters.
    /// </summary>
    /// <param name="message">The descriptive message to log.</param>
    /// <param name="args">Additional parameters to include in the structured log entry.</param>
    public void LogError(string message, params object?[] args);

    /// <summary>
    /// Logs an error message with an associated exception and additional structured parameters.
    /// </summary>
    /// <param name="exception">The exception to log (can be null).</param>
    /// <param name="message">The descriptive message to log.</param>
    /// <param name="args">Additional parameters to include in the structured log entry.</param>
    public void LogError(Exception? exception, string message, params object?[] args);

    /// <summary>
    /// Logs an error message with an exception and structured parameters.
    /// </summary>
    /// <param name="exception">The exception to log.</param>
    /// <param name="message">The message template string.</param>
    /// <param name="argument">The arguments of logger</param>
    /// <param name="args">Additional parameters to include in the structured log.</param>
    public void LogError(Exception? exception, string message, LoggerArgument argument, params object?[] args);
}