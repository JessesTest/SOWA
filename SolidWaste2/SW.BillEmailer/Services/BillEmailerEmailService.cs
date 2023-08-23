using Common.Extensions;
using Common.Services.Email;
using Microsoft.Extensions.Options;
using System.Text;

namespace SW.BillEmailer.Services;

public class BillEmailerEmailService
{
    private readonly SendEmailSettings settings;
    private readonly ISendGridService emailService;

    public BillEmailerEmailService(
        IOptions<SendEmailSettings> options,
        ISendGridService emailService)
    {
        this.settings = options.Value;
        this.emailService = emailService;
    }

    internal record AttachmentTemp(FileInfo File, string FileName);

    public async Task SendEmail(BillEmailerContext context)
    {
        context.CloseFiles();

        var hasError = context.BillEmailerExceptionFile.Exists && context.BillEmailerExceptionFile.Length > 0;

        string subject = hasError ?
            "Error Processing SW Bill Emails" :
            "SW Bill Emailer - " + context.Process_Date_Beg.ToString("MMM") + " " + context.Process_Date_Beg.ToString().PadLeft(4, '0');

        StringBuilder text = new(1024);

        text.AppendLine(hasError ?
            "Error; Please see attached files" :
            "Success; Please see attached files")
            .AppendLine()
            .AppendLine($"Run Time:         {DateTime.Now}")
            .AppendLine($"Beg Date:         {context.Process_Date_Beg}")
            .AppendLine($"End Date:         {context.Process_Date_End}");

        string[] tos = hasError ? settings.BillEmailerException : settings.BillEmailerSummary;

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

    private static async Task<ICollection<AttachmentDto>> CreateAttachments(BillEmailerContext context)
    {
        List<AttachmentDto> value = new();

        var files = new[]
        {
            new AttachmentTemp(context.BillEmailerExceptionFile, "SW_Bill_Emailer_Error_Rpt.txt"),
            new AttachmentTemp(context.BillEmailerSummaryFile, "SW_Bill_Emailer_Summary_Rpt.txt")
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
    public string[] BillEmailerSummary { get; set; }
    public string[] BillEmailerException { get; set; }
}
