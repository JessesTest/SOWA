using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SendGrid;
using SendGrid.Helpers.Mail;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace Common.Services.Email;

public class SendGridService : ISendGridService
{
    private readonly string _apiKey;
    private readonly ILogger<SendGridService> _logger;

    public SendGridService(IConfiguration configuration, ILogger<SendGridService> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(configuration));
        _apiKey = GetApiKey(configuration ?? throw new ArgumentNullException(nameof(configuration)));

        if (string.IsNullOrWhiteSpace(_apiKey))
            _logger.LogError("Missing SendGrid key");
    }

    private static string GetApiKey(IConfiguration configuration)
    {
        // local, dev, uat, prod have different keys

        var env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
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

        var message = new SendGridMessage();

        if(dto.Cc != null)
        {
            foreach(var cc in dto.Cc)
            {
                message.AddCc(cc.Address, cc.Name);
            }
        }

        if(dto.Bcc != null)
        {
            foreach(var bcc in dto.Bcc)
            {
                message.AddBcc(bcc.Address, bcc.Name);
            }
        }

        message.SetFrom(dto.From.Address, dto.From.Name);
        if(dto.ReplyTo!= null)
        {
            message.SetReplyTo(new EmailAddress(dto.ReplyTo.Address, dto.ReplyTo.Name));
        }

        message.SetSubject(dto.Subject);
        if (!string.IsNullOrWhiteSpace(dto.TextContent))
            message.AddContent("text/plain", dto.TextContent);

        if (!string.IsNullOrWhiteSpace(dto.HtmlContent))
            message.AddContent("text/html", dto.HtmlContent);

        message.AddTos(dto.To.Select(t => new EmailAddress(t.Address, t.Name)).ToList());

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

        var client = GetClient(_apiKey);
        var response = await client.SendEmailAsync(message);

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogWarning(JsonSerializer.Serialize(response));
        }
    }
}
