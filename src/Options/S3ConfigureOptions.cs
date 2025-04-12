using Infrastructure.Configuration;

namespace Infrastructure.Options;

/// <summary>
/// Class for configuring Amazon S3 settings.
/// </summary>
public sealed class S3ConfigureOptions : Validator
{
    /// <summary>
    /// Gets or sets the access key for Amazon S3.
    /// </summary>
    /// <value>The access key for Amazon S3.</value>
    public string AccessKey { internal get; set; } = null!;

    /// <summary>
    /// Gets or sets the secret key for Amazon S3.
    /// </summary>
    /// <value>The secret key for Amazon S3.</value>
    public string SecretKey { internal get; set; } = null!;

    /// <summary>
    /// Gets or sets the region for Amazon S3.
    /// </summary>
    /// <value>The region for Amazon S3.</value>
    public string Region { internal get; set; } = null!;

    /// <summary>
    /// Gets or sets the endpoint for the S3 storage provider.
    /// </summary>
    /// <value>The endpoint for the S3 storage provider.</value>
    /// <remarks>
    /// This property is required for Yandex Object Storage and should be set to the provider's endpoint (e.g., "https://storage.yandexcloud.net").
    /// For Amazon S3, this property is optional and can be left null.
    /// </remarks>
    public string Endpoint { internal get; set; } = null!;

    /// <summary>
    /// Validates whether the instance is configured correctly.
    /// </summary>
    /// <returns><c>true</c> if the configuration is valid; otherwise, <c>false</c>.</returns>
    /// <remarks>
    /// The configuration is considered valid if:
    /// - The <see cref="AccessKey"/> property is not null or empty.
    /// - The <see cref="SecretKey"/> property is not null or empty.
    /// - The <see cref="Region"/> property is not null or empty.
    /// </remarks>
    public override bool IsValidConfigure()
    {
        if (string.IsNullOrWhiteSpace(AccessKey) || string.IsNullOrWhiteSpace(SecretKey) || string.IsNullOrWhiteSpace(Region) || string.IsNullOrWhiteSpace(Endpoint))
            return false;

        return true;
    }
}
