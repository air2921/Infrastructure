namespace Infrastructure;

/// <summary>
/// Provides immutable constants and configuration values used throughout the application.
/// </summary>
/// <remarks>
/// This static class contains nested classes that group related constants logically.
/// All values are compile-time constants and cannot be modified at runtime.
/// </remarks>
public static class Immutable
{
    /// <summary>
    /// Contains constants related to ASP.NET Core configuration.
    /// </summary>
    public static class ASPNETCore
    {
        /// <summary>
        /// The environment variable name used by ASP.NET Core to determine the current environment.
        /// </summary>
        public const string AspNetCoreEnv = "ASPNETCORE_ENVIRONMENT";

        /// <summary>
        /// The property name used to access the environment in IWebHostEnvironment.
        /// </summary>
        public const string EnvProperty = "Environment";
    }

    /// <summary>
    /// Contains timeout values (in seconds) for various repository operations.
    /// </summary>
    public static class RepositoryTimeout
    {
        /// <summary>Timeout for GetRange operations (20 seconds).</summary>
        public const int GetRangeAwait = 20;

        /// <summary>Timeout for GetByFilter operations (20 seconds).</summary>
        public const int GetByFilterAwait = 20;

        /// <summary>Timeout for GetById operations (20 seconds).</summary>
        public const int GetByIdAwait = 20;

        /// <summary>Timeout for Add operations (20 seconds).</summary>
        public const int AddAwait = 20;

        /// <summary>Timeout for AddRange operations (20 seconds).</summary>
        public const int AddRangeAwait = 20;

        /// <summary>Timeout for RemoveById operations (20 seconds).</summary>
        public const int RemoveByIdAwait = 20;

        /// <summary>Timeout for RemoveRange operations (90 seconds).</summary>
        public const int RemoveRangeAwait = 90;

        /// <summary>Timeout for RemoveByFilter operations (20 seconds).</summary>
        public const int RemoveByFilterAwait = 20;

        /// <summary>Timeout for Update operations (20 seconds).</summary>
        public const int UpdateAwait = 20;

        /// <summary>Timeout for UpdateRange operations (60 seconds).</summary>
        public const int UpdateRangeAwait = 60;

        /// <summary>Timeout for Restore operations (20 seconds).</summary>
        public const int RestoreAwait = 20;

        /// <summary>Timeout for RestoreRange operations (60 seconds).</summary>
        public const int RestoreRangeAwait = 60;
    }

    /// <summary>
    /// Contains validation parameters and constraints.
    /// </summary>
    public static class ValidationParameter
    {
        /// <summary>Minimum allowed length for codes (1 character).</summary>
        public const int MinCodeLength = 1;

        /// <summary>Maximum allowed length for codes (10 characters).</summary>
        public const int MaxCodeLength = 10;

        /// <summary>Minimum allowed length for GUID combinations (1 combines).</summary>
        public const int MinGuidCombineLength = 1;

        /// <summary>Maximum allowed length for GUID combinations (10 combines).</summary>
        public const int MaxGuidCombineLength = 10;
    }

    /// <summary>
    /// Contains character sets used in the application.
    /// </summary>
    public static class Char
    {
        /// <summary>
        /// Characters considered easily readable (excluding visually similar characters like I, 1, O, 0).
        /// </summary>
        public const string ReadebleChars = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";

        /// <summary>
        /// Valid hexadecimal characters (0-9, A-F).
        /// </summary>
        public const string HexChars = "0123456789ABCDEF";
    }

    /// <summary>
    /// Contains regular expression patterns used for validation.
    /// </summary>
    public static class RegularExpression
    {
        /// <summary>
        /// Regular expression pattern for validating phone numbers in E.164 format.
        /// Allows optional '+' prefix and 1-15 digits following the international call prefix.
        /// </summary>
        public const string PhoneNumber = @"^\+?[1-9]\d{1,14}$";
    }
}
