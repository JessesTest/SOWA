using Common.Extensions;
using Common.Services.Email;
using Microsoft.Extensions.Options;
using System.Text;

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
        context.CloseFiles();

        var hasError = context.ExceptionFile.Exists && context.ExceptionFile.Length > 0;

        string subject = hasError ?
            "Error in SOWA Cash Receipts Job" :
            "SOWA Cash Receipts Job Details";

        StringBuilder text = new(1024);

        text.AppendLine(hasError ?
            "Error; Please see attached files" :
            "Success; Please see attached files")
            .AppendLine()
            .AppendLine($"Run Time:          {context.CashReceiptBeginDatetime}")
            .AppendLine($"Cash Receipt Date: {context.CashReceiptForDate:MM/dd/yyyy}")
            .AppendLine($"Payments Found:    {context.TotalPaymentsFound}");

        string[] tos = hasError ? settings.Error : settings.Good;
        
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

    private static async Task<ICollection<AttachmentDto>> CreateAttachments(CashReceiptContext context)
    {
        List<AttachmentDto> value = new();

        AttachmentTemp[] files = new[]
        {
            new AttachmentTemp(context.ExceptionFile, "SW_Cash_Receipt_Err_Rpt.txt"),
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
    public string[] Good { get; set; }
    public string[] Error { get; set; }
}
