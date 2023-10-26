using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SendGrid;
using SendGrid.Helpers.Mail;
using System.Text.Json;

namespace Common.Services.Email;

public class SendGridService : ISendGridService
{
    private readonly string _apiKey;
    private readonly ILogger<SendGridService> _logger;
    private readonly string _defaultFrom;
    private readonly string _defaultSubject;

    public SendGridService(IConfiguration configuration, ILogger<SendGridService> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(configuration));
        _apiKey = GetApiKey(configuration ?? throw new ArgumentNullException(nameof(configuration)));

        if (string.IsNullOrWhiteSpace(_apiKey))
            _logger.LogError("Missing SendGrid key");

        _defaultFrom = configuration.GetValue<string>("SendGrid:DefaultFrom");
        _defaultSubject = configuration.GetValue<string>("SendGrid:DefaultSubject");
    }

    private static string GetApiKey(IConfiguration configuration)
    {
        // local, dev, uat, prod have different keys

        string env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");

        if (string.IsNullOrWhiteSpace(env))
        {
            env = Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT");
            if (string.IsNullOrWhiteSpace(env))
                env = "Local";
        }
        return configuration.GetValue<string>($"SENDGRID_{env.ToUpper()}_KEY");
    }

    private static SendGridClient GetClient(string apiKey)
    {
        return new SendGridClient(apiKey);
    }

    public async Task SendMultipleEmails(SendEmailDto dto)
    {
        _ = dto ?? throw new ArgumentNullException(nameof(dto));

        foreach (var to in dto.To)
        {
            await SendSingleEmail(new SendEmailDto
            {
                Attachments = dto.Attachments,
                From = dto.From,
                HtmlContent = dto.HtmlContent,
                Subject = dto.Subject,
                TextContent = dto.TextContent,
                To = new List<SendGridEmailAddress> { to }
            });
        }
    }

    public async Task SendSingleEmail(SendEmailDto dto)
    {
        _ = dto ?? throw new ArgumentNullException(nameof(dto));

        #region OldWay
        //var message = new SendGridMessage();

        //if(dto.Cc != null)
        //{
        //    foreach(var cc in dto.Cc)
        //    {
        //        message.AddCc(cc.Address, cc.Name);
        //    }
        //}

        //if(dto.Bcc != null)
        //{
        //    foreach(var bcc in dto.Bcc)
        //    {
        //        message.AddBcc(bcc.Address, bcc.Name);
        //    }
        //}

        //if (dto.From != null)
        //    message.SetFrom(dto.From.Address, dto.From.Name);
        //else
        //    message.SetFrom(_defaultFrom);

        //if(dto.ReplyTo!= null)
        //{
        //    message.SetReplyTo(new EmailAddress(dto.ReplyTo.Address, dto.ReplyTo.Name));
        //}

        //if (!string.IsNullOrWhiteSpace(dto.Subject))
        //    message.SetSubject(dto.Subject);
        //else
        //    message.SetSubject(_defaultSubject);

        //if (!string.IsNullOrWhiteSpace(dto.TextContent))
        //    message.AddContent("text/plain", dto.TextContent);

        //if (!string.IsNullOrWhiteSpace(dto.HtmlContent))
        //    message.AddContent("text/html", dto.HtmlContent);

        //message.AddTos(dto.To.Select(t => new EmailAddress(t.Address, t.Name)).ToList());

        //if (dto.Attachments != null && dto.Attachments.Any())
        //{
        //    foreach (var a in dto.Attachments)
        //    {
        //        message.AddAttachment(new Attachment
        //        {
        //            Content = a.Content,
        //            ContentId = a.DispositionInline ? a.ContentId : null,
        //            Disposition = a.DispositionInline ? "inline" : "attachment",
        //            Filename = a.FileName,
        //            Type = a.ContentType
        //        });
        //    }
        //}
        #endregion

        SendGridMessage validatedMessage = ValidateSendEmailDTO(dto);

        SendGridClient client = GetClient(_apiKey);
        Response response = await client.SendEmailAsync(validatedMessage);

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogWarning(JsonSerializer.Serialize(response));
        }
    }

    private SendGridMessage ValidateSendEmailDTO(SendEmailDto dto)
    {
        var message = new SendGridMessage();
        #region OldWay
        //if (dto.Cc != null)
        //{
        //    foreach (var cc in dto.Cc)
        //    {
        //        message.AddCc(cc.Address, cc.Name);
        //    }
        //}

        //if (dto.Bcc != null)
        //{
        //    foreach (var bcc in dto.Bcc)
        //    {
        //        message.AddBcc(bcc.Address, bcc.Name);
        //    }
        //}

        //if (dto.From != null)
        //    message.SetFrom(dto.From.Address, dto.From.Name);
        //else
        //    message.SetFrom(_defaultFrom);

        //if (dto.ReplyTo != null)
        //{
        //    message.SetReplyTo(new EmailAddress(dto.ReplyTo.Address, dto.ReplyTo.Name));
        //}

        //if (!string.IsNullOrWhiteSpace(dto.Subject))
        //    message.SetSubject(dto.Subject);
        //else
        //    message.SetSubject(_defaultSubject);

        //if (!string.IsNullOrWhiteSpace(dto.TextContent))
        //    message.AddContent("text/plain", dto.TextContent);

        //if (!string.IsNullOrWhiteSpace(dto.HtmlContent))
        //    message.AddContent("text/html", dto.HtmlContent);


        //if (dto.Attachments != null && dto.Attachments.Any())
        //{
        //    foreach (var a in dto.Attachments)
        //    {
        //        message.AddAttachment(new Attachment
        //        {
        //            Content = a.Content,
        //            ContentId = a.DispositionInline ? a.ContentId : null,
        //            Disposition = a.DispositionInline ? "inline" : "attachment",
        //            Filename = a.FileName,
        //            Type = a.ContentType
        //        });
        //    }
        //}
        //message.SendAt // UNIX time
        #endregion

        message = ValidateCC(dto,message);
        message = ValidateBCC(dto,message);
        message = ValidateFrom(dto,message);
        message = ValidateReplyTo(dto,message);
        message = ValidateSubject(dto,message);
        message = ValidateAttachments(dto,message);
        message = ValidateTextContent(dto,message);
        message = ValidateHtmlContent(dto,message);

        message.AddTos(dto.To.Select(t => new EmailAddress(t.Address, t.Name)).ToList());

        return message;
    }

    private SendGridMessage ValidateCC(SendEmailDto dto, SendGridMessage message)
    {
        if (dto.Cc != null)
        {
            foreach (var cc in dto.Cc)
            {
                message.AddCc(cc.Address, cc.Name);
            }
        }

        return message;
    }

    private SendGridMessage ValidateBCC(SendEmailDto dto, SendGridMessage message)
    {
        if (dto.Bcc != null)
        {
            foreach (var bcc in dto.Bcc)
            {
                message.AddBcc(bcc.Address, bcc.Name);
            }
        }

        return message;
    }

    private SendGridMessage ValidateFrom(SendEmailDto dto, SendGridMessage message)
    {
        if (dto.From != null)
        {
            message.SetFrom(dto.From.Address, dto.From.Name);
        }
        else
        {
            message.SetFrom(_defaultFrom);
        }

        return message;
    }

    private SendGridMessage ValidateReplyTo(SendEmailDto dto, SendGridMessage message)
    {
        if (dto.ReplyTo != null)
        {
            message.SetReplyTo(new EmailAddress(dto.ReplyTo.Address, dto.ReplyTo.Name));
        }

        return message;
    }

    private SendGridMessage ValidateSubject(SendEmailDto dto, SendGridMessage message)
    {
        if (!string.IsNullOrWhiteSpace(dto.Subject))
        {
            message.SetSubject(dto.Subject);
        }
        else
        {
            message.SetSubject(_defaultSubject);
        }

        return message;
    }

    private SendGridMessage ValidateAttachments(SendEmailDto dto, SendGridMessage message)
    {
        if (dto.Attachments != null && dto.Attachments.Any())
        {
            foreach (var a in dto.Attachments)
            {
                message.AddAttachment(new Attachment
                {
                    Content = a.Content,
                    ContentId = a.DispositionInline ? a.ContentId : null,
                    Disposition = a.DispositionInline ? "inline" : "attachment",
                    Filename = a.FileName,
                    Type = a.ContentType
                });
            }
        }
        //message.SendAt // UNIX time
        return message;
    }

    private SendGridMessage ValidateTextContent(SendEmailDto dto, SendGridMessage message)
    {
        if (!string.IsNullOrWhiteSpace(dto.TextContent))
            message.AddContent("text/plain", dto.TextContent);

        return message;
    }

    private SendGridMessage ValidateHtmlContent(SendEmailDto dto, SendGridMessage message)
    {
        if (!string.IsNullOrWhiteSpace(dto.HtmlContent))
            message.AddContent("text/html", dto.HtmlContent);

        return message;
    }
}
