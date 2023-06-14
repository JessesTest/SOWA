using Common.Extensions;
using Common.Services.Email;
using Microsoft.Extensions.Options;

namespace SW.IfasCashReceipt.Services;

public sealed class CashReceiptEmailService
{
    private readonly SendEmailSettings settings;
    private readonly ISendGridService emailService;

    public CashReceiptEmailService(
        IOptions<SendEmailSettings> options,
        ISendGridService emailService)
    {
        settings = options.Value;
        this.emailService = emailService;
    }

    internal record AttachmentTemp(FileInfo File, string FileName);

    public async Task SendEmail(CashReceiptContext context)
    {
        var hasError = context.ErrRptFile.Exists;

        string subject = hasError ?
            "Error in SOWA Cash Receipts Job" :
            "SOWA Cash Receipts Job Details";

        string text = hasError ?
            "Error; Please see attached files" :
            "Success; Please see attached files";

        string[] tos = hasError ? settings.Error : settings.Good;
        
        var attachments = await CreateAttachments(context);

        SendEmailDto email = new()
        {
            Attachments = attachments,
            HtmlContent = null,
            Subject = subject,
            TextContent = text
        };
        foreach (var to in tos)
        {
            email.AddTo(to);
        }

        await emailService.SendSingleEmail(email);
    }

    private static async Task<ICollection<AttachmentDto>> CreateAttachments(CashReceiptContext context)
    {
        context.CloseFiles();

        List<AttachmentDto> value = new();

        AttachmentTemp[] files = new[]
        {
            new AttachmentTemp(context.ErrRptFile, "SW_Cash_Receipt_Err_Rpt.txt"),
            new AttachmentTemp(context.ReportFile, "SW_Cash_Receipt_Rpt.txt"),
            new AttachmentTemp(context.ErrorFile, "Error.txt"),
            new AttachmentTemp(context.GoodFile, "Good.txt")
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
        if (!file.Exists || file.Length == 0)
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

        return attachment;
    }
}

public class SendEmailSettings
{
    public string[] Good { get; set; }
    public string[] Error { get; set; }
}
