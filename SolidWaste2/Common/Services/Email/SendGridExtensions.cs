using Twilio.Rest.Api.V2010.Account.Usage.Record;

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

    public static SendEmailDto SetFrom(this SendEmailDto dto, string email, string name = null)
    {
        dto.From = new SendGridEmailAddress(email, name);

        return dto;
    }

    public static SendEmailDto AddTo(this SendEmailDto dto, string email, string name = null)
    {
        ICollection<SendGridEmailAddress> tos;

        if (dto.To == null)
        {
            tos = new List<SendGridEmailAddress>();
            dto.To = tos;
        }
        else
        {
            tos = (ICollection<SendGridEmailAddress>)dto.To;
        }
        tos.Add(new SendGridEmailAddress(email, name));
        return dto;
    }
}
