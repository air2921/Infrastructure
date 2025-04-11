using Amazon.S3;
using Infrastructure.Data_Transfer_Object.Base;

namespace Infrastructure.Data_Transfer_Object.S3;

/// <summary>
/// Represents details of a pre-signed URL for temporary access to an S3 object.
/// This class extends <see cref="S3ObjectDetails"/> with URL-specific access information.
/// </summary>
/// <remarks>
/// Used for generating time-limited access URLs that can be shared without requiring AWS credentials.
/// The <see cref="Url"/> becomes invalid after the <see cref="Expires"/> timestamp.
/// </remarks>
public class S3ObjectUrlDetails : S3ObjectDetails
{
    /// <summary>
    /// Gets or sets the generated pre-signed URL for accessing the object.
    /// </summary>
    public required string Url { get; set; }

    /// <summary>
    /// Gets or sets the expiration date and time of the pre-signed URL.
    /// </summary>
    /// <remarks>
    /// All times are in UTC. Attempts to use the URL after this time will return 403 Forbidden.
    /// </remarks>
    public DateTime Expires { get; set; }

    /// <summary>
    /// Gets or sets the HTTP verb permitted by the pre-signed URL.
    /// </summary>
    /// <remarks>
    /// Typically GET for downloads or PUT for uploads. Other verbs may be restricted by S3 policies.
    /// </remarks>
    public HttpVerb Verb { get; set; }
}
