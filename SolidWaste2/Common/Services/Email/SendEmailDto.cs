namespace Common.Services.Email;

public class SendEmailDto
{
    public SendGridEmailAddress From { get; set; }
    public SendGridEmailAddress ReplyTo { get; set; }
    public IEnumerable<SendGridEmailAddress> To { get; set; }
    public IEnumerable<SendGridEmailAddress> Cc { get; set; }
    public IEnumerable<SendGridEmailAddress> Bcc { get; set; }

    public string Subject { get; set; }
    public string HtmlContent { get; set; }
    public string TextContent { get; set; }
    public IEnumerable<AttachmentDto> Attachments { get; set; }
}
