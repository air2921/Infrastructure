using Infrastructure.Configuration;
using Infrastructure.Enums;
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
    /// Gets or sets the allowed usage mode for Twilio phone numbers.
    /// </summary>
    /// <value>The usage mode that determines whether numbers can be used for calls, messages, or both.</value>
    /// <remarks>
    /// This property controls how phone numbers from this configuration can be used.
    /// When set, it validates that numbers are properly configured in Twilio for their designated purpose.
    /// </remarks>
    public TwilioPhoneNumberMode PhoneMode { internal get; set; }

    /// <summary>
    /// Gets or sets the Twilio Account SID.
    /// </summary>
    /// <value>The Twilio Account SID.</value>
    /// <remarks>
    /// The Account SID is used to identify your Twilio account.
    /// This identifier is required for authentication and to interact with the Twilio API.
    /// </remarks>
    public string AccountSid { internal get; set; } = null!;

    /// <summary>
    /// Gets or sets the username for authentication with Twilio.
    /// </summary>
    /// <value>The username for authentication.</value>
    /// <remarks>
    /// This is the username used during the authentication process via Basic Authentication.
    /// Typically matches your Account SID for Twilio API authentication.
    /// </remarks>
    public string Username { internal get; set; } = null!;

    /// <summary>
    /// Gets or sets the password for authentication with Twilio.
    /// </summary>
    /// <value>The password for authentication.</value>
    /// <remarks>
    /// This password is used during the authentication process along with the username for Basic Authentication.
    /// Typically matches your Auth Token for Twilio API authentication.
    /// </remarks>
    public string Password { internal get; set; } = null!;

    /// <summary>
    /// Gets or sets the collection of phone numbers that can be used for voice calls.
    /// </summary>
    /// <value>An enumerable of phone numbers in E.164 format.</value>
    /// <remarks>
    /// These numbers should be specifically configured in Twilio with voice capabilities enabled.
    /// The numbers are validated against the <see cref="PhoneMode"/> setting when used.
    /// </remarks>
    public IEnumerable<string> CallPhoneNumbers { internal get; set; } = [];

    /// <summary>
    /// Gets or sets the collection of phone numbers that can be used for messaging (SMS/MMS).
    /// </summary>
    /// <value>An enumerable of phone numbers in E.164 format.</value>
    /// <remarks>
    /// These numbers should be specifically configured in Twilio with SMS/MMS capabilities enabled.
    /// The numbers are validated against the <see cref="PhoneMode"/> setting when used.
    /// </remarks>
    public IEnumerable<string> MessagePhoneNumber { internal get; set; } = [];

    /// <summary>
    /// Validates whether the Twilio configuration is correct.
    /// </summary>
    /// <returns><c>true</c> if the configuration is valid; otherwise, <c>false</c>.</returns>
    /// <remarks>
    /// The configuration is considered valid if:
    /// - The <see cref="AccountSid"/> is not null or empty.
    /// - The <see cref="Username"/> is not null or empty.
    /// - The <see cref="Password"/> is not null or empty.
    /// - The <see cref="CallPhoneNumbers"/> is not empty, and matches a valid phone number format.
    /// - The <see cref="MessagePhoneNumber"/> is not empty, and matches a valid phone number format.
    /// </remarks>
    public override bool IsValidConfigure()
    {
        if (string.IsNullOrWhiteSpace(AccountSid) || string.IsNullOrWhiteSpace(Username) || string.IsNullOrWhiteSpace(Password))
            return false;

        if (!CallPhoneNumbers.Any() && (PhoneMode == TwilioPhoneNumberMode.All || PhoneMode == TwilioPhoneNumberMode.CallOnly))
            return false;

        if (!MessagePhoneNumber.Any() && (PhoneMode == TwilioPhoneNumberMode.All || PhoneMode == TwilioPhoneNumberMode.MessageOnly))
            return false;

        var phones = CallPhoneNumbers.Concat(MessagePhoneNumber);
        foreach (var phone in phones)
        {
            if (!Regex.IsMatch(phone, InfrastructureImmutable.RegularExpression.PhoneNumber))
                return false;
        }

        return true;
    }
}
