namespace Infrastructure.Data_Transfer_Object.Base;

/// <summary>
/// Represents base details of an S3 object.
/// This abstract class provides the fundamental identifier for objects in S3 storage.
/// </summary>
/// <remarks>
/// All S3 object detail representations inherit from this base class to ensure consistent key-based identification.
/// The <see cref="Key"/> property follows S3's path-like naming convention (e.g., 'folder/subfolder/file.txt').
/// </remarks>
public abstract class S3ObjectDetails
{
    /// <summary>
    /// Gets or sets the object key representing the full path within the S3 bucket.
    /// </summary>
    public required string Key { get; set; }
}
