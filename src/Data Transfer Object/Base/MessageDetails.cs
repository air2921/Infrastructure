using System.ComponentModel.DataAnnotations;

namespace Infrastructure.Data_Transfer_Object.Base;

/// <summary>
/// Abstract class representing the details of a message.
/// </summary>
public abstract class MessageDetails
{
    /// <summary>
    /// Gets or sets the recipient of the message.
    /// </summary>
    [Required]
    public virtual string To { get; set; } = null!;
}
