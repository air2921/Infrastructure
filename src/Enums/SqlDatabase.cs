namespace Infrastructure.Enums;

/// <summary>
/// Represents the types of SQL databases supported by the application.
/// </summary>
public enum SqlDatabase
{
    /// <summary>
    /// PostgreSQL database.
    /// </summary>
    PostgreSql,

    /// <summary>
    /// Microsoft SQL Server database.
    /// </summary>
    SqlServer,

    /// <summary>
    /// SQLite database.
    /// </summary>
    SqlLite
}