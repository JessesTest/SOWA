namespace Common.Services.Email;

public static class SendGridExtensions
{
    public static Task SendSingleEmails(
        this ISendGridService service,
        SendGridEmailAddress from,
        IEnumerable<SendGridEmailAddress> to,
        string subject,
        string textContent = null,
        string htmlContent = null,
        IEnumerable<AttachmentDto> attachments = null)
    {
        _ = service ?? throw new ArgumentNullException(nameof(service));

        return service.SendSingleEmail(new SendEmailDto
        {
            Attachments = attachments,
            From = from,
            HtmlContent = htmlContent,
            Subject = subject,
            TextContent = textContent,
            To = to
        });
    }

    public static Task SendSingleEmail(
        this ISendGridService service,
        SendGridEmailAddress from,
        SendGridEmailAddress to,
        string subject,
        string textContent = null,
        string htmlContent = null,
        IEnumerable<AttachmentDto> attachments = null)
    {
        _ = service ?? throw new ArgumentNullException(nameof(service));

        return service.SendSingleEmail(new SendEmailDto
        {
            Attachments = attachments,
            From = from,
            HtmlContent = htmlContent,
            Subject = subject,
            TextContent = textContent,
            To = new[] { to }
        });
    }
}
