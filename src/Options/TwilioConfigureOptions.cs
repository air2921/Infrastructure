using Infrastructure.Configuration;
using System.Text.RegularExpressions;

namespace Infrastructure.Options;

/// <summary>
/// Class for configuring Twilio SMS settings.
/// </summary>
/// <remarks>
/// This class contains properties for configuring Twilio API credentials such as Account SID, username, password, and phone number.
/// It also provides a method to validate if the configuration is correctly set.
/// </remarks>
public class TwilioConfigureOptions : Validator
{
    /// <summary>
    /// Gets or sets the Twilio Account SID.
    /// </summary>
    /// <value>The Twilio Account SID.</value>
    /// <remarks>
    /// The Account SID is used to identify your Twilio account.
    /// This identifier is required for authentication and to interact with the Twilio API.
    /// </remarks>
    public string AccountSid { get; set; } = null!;

    /// <summary>
    /// Gets or sets the username for authentication with Twilio.
    /// </summary>
    /// <value>The username for authentication.</value>
    /// <remarks>
    /// This is the username used during the authentication process via Basic Authentication.
    /// </remarks>
    public string Username { get; set; } = null!;

    /// <summary>
    /// Gets or sets the password for authentication with Twilio.
    /// </summary>
    /// <value>The password for authentication.</value>
    /// <remarks>
    /// This password is used during the authentication process along with the username for Basic Authentication.
    /// </remarks>
    public string Password { get; set; } = null!;

    /// <summary>
    /// Gets or sets the phone number to be used for sending SMS messages.
    /// </summary>
    /// <value>The phone number to send SMS messages from.</value>
    /// <remarks>
    /// The phone number must be in a valid international format (e.g., +1XXXXXXXXXX).
    /// This number is used as the sender when sending messages through the Twilio API.
    /// </remarks>
    public string PhoneNumber { get; set; } = null!;

    /// <summary>
    /// Validates whether the Twilio configuration is correct.
    /// </summary>
    /// <returns><c>true</c> if the configuration is valid; otherwise, <c>false</c>.</returns>
    /// <remarks>
    /// The configuration is considered valid if:
    /// - The <see cref="AccountSid"/> is not null or empty.
    /// - The <see cref="Username"/> is not null or empty.
    /// - The <see cref="Password"/> is not null or empty.
    /// - The <see cref="PhoneNumber"/> is not null, empty, and matches a valid phone number format.
    /// </remarks>
    public override bool IsValidConfigure()
    {
        // Check if all required fields are provided
        if (string.IsNullOrWhiteSpace(AccountSid) || string.IsNullOrWhiteSpace(Username) || string.IsNullOrWhiteSpace(Password) || string.IsNullOrWhiteSpace(PhoneNumber))
            return false;

        // Check if the phone number matches the international phone format
        var phonePattern = @"^\+?[1-9]\d{1,14}$";
        if (!Regex.IsMatch(PhoneNumber, phonePattern))
            return false;

        return true;
    }
}
