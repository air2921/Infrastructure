namespace Infrastructure.Data_Transfer_Object.Authorization;

/// <summary>
/// Represents details of a refresh token used for authorization.
/// This class includes information about the length of the token, defined by the number of GUIDs combined to create it.
/// </summary>
/// <remarks>
/// This class is used to encapsulate refresh token-specific details for authorization purposes.
/// The <see cref="CombineCount"/> property determines how many GUIDs are concatenated to form the token.
/// </remarks>
public class RefreshDetails : AuthorizationDetails
{
    /// <summary>
    /// Gets or sets the number of GUIDs to combine when generating the refresh token.
    /// This determines the length and complexity of the token.
    /// </summary>
    public int CombineCount { get; set; } = 3;
}