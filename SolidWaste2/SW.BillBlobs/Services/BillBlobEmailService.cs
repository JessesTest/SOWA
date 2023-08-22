using Common.Extensions;
using Common.Services.Email;
using Microsoft.Extensions.Options;
using System.Text;

namespace SW.BillBlobs.Services;

public class BillBlobEmailService
{
    private readonly SendEmailSettings settings;
    private readonly ISendGridService emailService;

    public BillBlobEmailService(
        IOptions<SendEmailSettings> options,
        ISendGridService emailService)
    {
        this.settings = options.Value;
        this.emailService = emailService;
    }

    internal record AttachmentTemp(FileInfo File, string FileName);

    public async Task SendEmail(BillBlobContext context)
    {
        context.CloseFiles();

        var hasError = context.BillBlobExceptionFile.Exists && context.BillBlobExceptionFile.Length > 0;

        string subject = hasError ?
            "Error Processing SW Bill Blobs" :
            "SW Bill Blobs Batch PDF - " + context.Mthly_Bill_Blob_Beg_DateTime.ToString("MMM") + " " + context.Mthly_Bill_Blob_Beg_DateTime.Year.ToString().PadLeft(4, '0');

        StringBuilder text = new(1024);

        text.AppendLine(hasError ?
            "Error; Please see attached files" :
            "Success; Please see attached files")
            .AppendLine()
            .AppendLine($"Run Time:         {DateTime.Now}")
            .AppendLine($"Beg Date:         {context.Mthly_Bill_Blob_Beg_DateTime}")
            .AppendLine($"End Date:         {context.Mthly_Bill_Blob_End_DateTime}");

        string[] tos = hasError ? settings.BillBlobException : settings.BillBlobSummary;

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

    private static async Task<ICollection<AttachmentDto>> CreateAttachments(BillBlobContext context)
    {
        List<AttachmentDto> value = new();

        var files = new[]
        {
            new AttachmentTemp(context.BillBlobExceptionFile, "SW_Bill_Blob_Error_Rpt.txt"),
            new AttachmentTemp(context.BillBlobSummaryFile, "SW_Bill_Blob_Summary_Rpt.txt")
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
    public string[] BillBlobSummary { get; set; }
    public string[] BillBlobException { get; set; }
}
