namespace Infrastructure.Enums;

/// <summary>
/// Specifies the allowed usage mode for a Twilio phone number.
/// </summary>
/// <remarks>
/// This enumeration defines how a Twilio phone number can be used in the application.
/// It helps enforce separation of concerns between different communication channels.
/// </remarks>
public enum TwilioPhoneNumberMode
{
    /// <summary>
    /// The phone number can only be used for voice calls.
    /// SMS/MMS functionality will be disabled for numbers with this mode.
    /// </summary>
    /// <remarks>
    /// When selected, the number should be configured in Twilio console with:
    /// - Voice capabilities enabled
    /// - SMS/MMS capabilities disabled
    /// </remarks>
    CallOnly,

    /// <summary>
    /// The phone number can only be used for sending and receiving text messages (SMS/MMS).
    /// Voice calling functionality will be disabled for numbers with this mode.
    /// </summary>
    /// <remarks>
    /// When selected, the number should be configured in Twilio console with:
    /// - SMS/MMS capabilities enabled
    /// - Voice capabilities disabled
    /// </remarks>
    MessageOnly,

    /// <summary>
    /// The phone number can be used for both voice calls and text messages.
    /// </summary>
    /// <remarks>
    /// When selected, the number should be configured in Twilio console with:
    /// - Both Voice and SMS/MMS capabilities enabled
    /// </remarks>
    All
}
