using MimeKit;

namespace Infrastructure.Data_Transfer_Object;

public class MailDetails
{
    public required string UsernameTo { get; set; }
    public required string EmailTo { get; set; }
    public required string Subject { get; set; }
    public required MimeEntity Entity { get; set; }
}
