namespace Infrastructure;

/// <summary>
/// Provides immutable constants and configuration values used throughout the application.
/// </summary>
/// <remarks>
/// This static class contains nested classes that group related constants logically.
/// All values are compile-time constants and cannot be modified at runtime.
/// </remarks>
internal static class InfrastructureImmutable
{
    /// <summary>
    /// Contains constants related to ASP.NET Core configuration.
    /// </summary>
    internal static class ASPNETCore
    {
        /// <summary>
        /// The environment variable name used by ASP.NET Core to determine the current environment.
        /// </summary>
        internal const string AspNetCoreEnv = "ASPNETCORE_ENVIRONMENT";

        /// <summary>
        /// The property name used to access the environment in IWebHostEnvironment.
        /// </summary>
        internal const string EnvProperty = "Environment";
    }

    /// <summary>
    /// Contains timeout values (in seconds) for various repository operations.
    /// </summary>
    internal static class RepositoryTimeout
    {
        /// <summary>Timeout for GetRange operations (20 seconds).</summary>
        internal const int GetRangeTimeout = 20;

        /// <summary>Timeout for GetByFilter operations (20 seconds).</summary>
        internal const int GetByFilterTimeout = 20;

        /// <summary>Timeout for GetById operations (20 seconds).</summary>
        internal const int GetByIdTimeout = 20;

        /// <summary>Timeout for Add operations (20 seconds).</summary>
        internal const int AddTimeout = 20;

        /// <summary>Timeout for AddRange operations (20 seconds).</summary>
        internal const int AddRangeTimeout = 20;

        /// <summary>Timeout for RemoveById operations (20 seconds).</summary>
        internal const int RemoveByIdTimeout = 20;

        /// <summary>Timeout for RemoveRange operations (90 seconds).</summary>
        internal const int RemoveRangeTimeout = 90;

        /// <summary>Timeout for RemoveByFilter operations (20 seconds).</summary>
        internal const int RemoveByFilterTimeout = 20;

        /// <summary>Timeout for Update operations (20 seconds).</summary>
        internal const int UpdateTimeout = 20;

        /// <summary>Timeout for UpdateRange operations (60 seconds).</summary>
        internal const int UpdateRangeTimeout = 60;

        /// <summary>Timeout for Restore operations (20 seconds).</summary>
        internal const int RestoreTimeout = 20;

        /// <summary>Timeout for RestoreRange operations (60 seconds).</summary>
        internal const int RestoreRangeTimeout = 60;
    }

    /// <summary>
    /// Contains validation parameters and constraints.
    /// </summary>
    internal static class ValidationParameter
    {
        /// <summary>Minimum allowed length for codes (1 character).</summary>
        internal const int MinCodeLength = 1;

        /// <summary>Maximum allowed length for codes (10 characters).</summary>
        internal const int MaxCodeLength = 10;

        /// <summary>Minimum allowed length for GUID combinations (1 combines).</summary>
        internal const int MinGuidCombineLength = 1;

        /// <summary>Maximum allowed length for GUID combinations (15 combines).</summary>
        internal const int MaxGuidCombineLength = 15;
    }

    /// <summary>
    /// Contains character sets used in the application.
    /// </summary>
    internal static class Char
    {
        /// <summary>
        /// Characters considered easily readable (excluding visually similar characters like I, 1, O, 0).
        /// </summary>
        internal const string ReadebleChars = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";

        /// <summary>
        /// Valid hexadecimal characters (0-9, A-F).
        /// </summary>
        internal const string HexChars = "0123456789ABCDEF";
    }

    /// <summary>
    /// Contains regular expression patterns used for validation.
    /// </summary>
    internal static class RegularExpression
    {
        /// <summary>
        /// Regular expression pattern for validating phone numbers in E.164 format.
        /// Allows optional '+' prefix and 1-15 digits following the international call prefix.
        /// </summary>
        internal const string PhoneNumber = @"^\+?[1-9]\d{1,14}$";
    }
}
