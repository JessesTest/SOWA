using Common.Extensions;
using Common.Services.Email;
using Microsoft.Extensions.Options;
using Microsoft.Graph;
using Newtonsoft.Json.Linq;
using System.Text;

namespace SW.BillGenerate.Services;

public class BillGenerateEmailService
{
    private readonly SendEmailSettings settings;
    private readonly ISendGridService emailService;

    public BillGenerateEmailService(
        IOptions<SendEmailSettings> options,
        ISendGridService emailService)
    {
        this.settings = options.Value;
        this.emailService = emailService;
    }

    internal record AttachmentTemp(FileInfo File, string FileName);

    public async Task SendEmail(BillGenerateContext context)
    {
        context.CloseFiles();

        var hasError = context.BillGenerateExceptionFile.Exists && context.BillGenerateExceptionFile.Length > 0;

        string subject = hasError ?
            "Error Generating SW Bills Batch PDF" :
            "SW Bill Generate Batch PDF - " + context.Mthly_Bill_Generate_Beg_DateTime.ToString("MMM") + " " + context.Mthly_Bill_Generate_Beg_DateTime.Year.ToString().PadLeft(4, '0');

        StringBuilder text = new(1024);

        text.AppendLine(hasError ?
            "Error; Please see attached files" :
            "Success; Please see attached files")
            .AppendLine()
            .AppendLine($"Run Time:         {DateTime.Now}")
            .AppendLine($"Beg Date:         {context.Mthly_Bill_Generate_Beg_DateTime}")
            .AppendLine($"End Date:         {context.Mthly_Bill_Generate_End_DateTime}");

        string[] tos = hasError ? settings.BillGenerateException : settings.BillGenerateBatchPdf;

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

    private static async Task<ICollection<AttachmentDto>> CreateAttachments(BillGenerateContext context)
    {
        List<AttachmentDto> value = new();

        var files = new[]
        {
            new AttachmentTemp(context.BillGenerateExceptionFile, "SW_Bill_Generate_Error_Rpt.txt"),
            new AttachmentTemp(context.BillGenerateBatchPdfFile, "SW_Bills_" + DateTime.Now.ToString("yyyy-MM-dd") + ".pdf")
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

        var content_type = "text/plain";
        if(fileName != "SW_Bill_Generate_Error_Rpt.txt") 
        {
            content_type = "application/pdf";
        }

        AttachmentDto attachment = new()
        {
            Content = bytes.ToBase64String(),
            ContentId = null,
            ContentType = content_type,
            DispositionInline = false,
            FileName = fileName
        };

        readStream.Close();

        return attachment;
    }
}

public class SendEmailSettings
{
    public string[] BillGenerateBatchPdf { get; set; }
    public string[] BillGenerateException { get; set; }
}
