using Common.Extensions;
using Common.Services.Email;
using Microsoft.Extensions.Options;
using Microsoft.Graph;
using Newtonsoft.Json.Linq;
using System.Text;

namespace SW.Billing.Services;

public class BillingEmailService
{
    private readonly SendEmailSettings settings;
    private readonly ISendGridService emailService;

    public BillingEmailService(
        IOptions<SendEmailSettings> options,
        ISendGridService emailService)
    {
        this.settings = options.Value;
        this.emailService = emailService;
    }

    internal record AttachmentTemp(FileInfo File, string FileName);

    public async Task SendEmail(BillingContext context)
    {
        context.CloseFiles();

        var hasError = context.BillingExceptionFile.Exists && context.BillingExceptionFile.Length > 0;

        string subject = hasError ?
            "Error Processing SW Bills" :
            "SW Billing Summary for " + context.Mthly_Bill_Beg_DateTime.ToString("MMM") + " " + context.Mthly_Bill_Beg_DateTime.Year.ToString().PadLeft(4, '0');

        StringBuilder text = new(1024);

        text.AppendLine(hasError ?
            "Error; Please see attached files" :
            "Success; Please see attached files")
            .AppendLine()
            .AppendLine($"Run Time:         {DateTime.Now}")
            .AppendLine($"Beg Date:         {context.Mthly_Bill_Beg_DateTime}")
            .AppendLine($"End Date:         {context.Mthly_Bill_End_DateTime}");

        string[] tos = hasError ? settings.BillingException : settings.BillingSummary;

        var attachments = await CreateAttachments(context);

        SendEmailDto email = new()
        {
            Attachments = attachments,
            HtmlContent = null,
            Subject = subject,
            TextContent = text.ToString()
        };
        foreach (var to in tos)
        {
            email.AddTo(to);
        }

        await emailService.SendSingleEmail(email);
    }

    private static async Task<ICollection<AttachmentDto>> CreateAttachments(BillingContext context)
    {
        List<AttachmentDto> value = new();

        var files = new[]
        {
            new AttachmentTemp(context.BillingExceptionFile, "SW_Billing_Error_Rpt.txt"),
            new AttachmentTemp(context.BillingSummaryFile, "SW_Billing_Summary_Rpt.txt")
        };
        foreach (var file in files)
        {
            var attachment = await CreateAttachment(file.File, file.FileName);
            if (attachment != null)
                value.Add(attachment);
        }
        return value;
    }

    private static async Task<AttachmentDto> CreateAttachment(FileInfo file, string fileName)
    {
        if (!file.Exists)
        {
            return null;
        }

        var bytesLength = (int)file.Length;
        var bytes = new byte[bytesLength];
        using var readStream = file.OpenRead();
        await readStream.ReadAsync(bytes, 0, bytesLength);

        AttachmentDto attachment = new()
        {
            Content = bytes.ToBase64String(),
            ContentId = null,
            ContentType = "text/plain",
            DispositionInline = false,
            FileName = fileName
        };

        readStream.Close();

        return attachment;
    }
}

public class SendEmailSettings
{
    public string[] BillingSummary { get; set; }
    public string[] BillingException { get; set; }
}