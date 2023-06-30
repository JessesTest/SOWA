using Common.Extensions;
using Common.Services.Email;
using Microsoft.Extensions.Options;
using System.Text;

namespace SW.WeeklyRental.Services;

public class WeeklyRentalEmailService
{
    private readonly SendEmailSettings settings;
    private readonly ISendGridService emailService;

    public WeeklyRentalEmailService(
        IOptions<SendEmailSettings> settings, 
        ISendGridService emailService)
    {
        this.settings = settings.Value;
        this.emailService = emailService;
    }

    public async Task SendEmail(WeeklyRentalContext context)
    {
        context.CloseFiles();

        var hasError = context.ExceptionFile.Exists && context.ExceptionFile.Length > 0;

        string subject = hasError ?
            "Error in SOWA Weekly Rental Billing" :
            "SOWA Weekly Rental Billing Good File";

        StringBuilder text = new(1024);

        text.AppendLine(hasError ?
            "Error; Please see attached files" :
            "Success; Please see attached files")
            .AppendLine()
            .AppendLine($"Run Time:             {DateTime.Now}")
            .AppendLine($"Process Date:         {context.CurrentDate:MM/dd/yyyy}")
            .AppendLine($"Transactions Created: {context.TransactionsCreated}");

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

    private static async Task<ICollection<AttachmentDto>> CreateAttachments(WeeklyRentalContext context)
    {
        List<AttachmentDto> value = new();

        var files = new[]
        {
            new AttachmentTemp(context.ExceptionFile, "SW_Weekly_Rental_Err_Rpt.txt"),
            //new AttachmentTemp(context.ReportFile, "SW_Weekly_Rental_Rpt.txt"),
            //new AttachmentTemp(context.ErrorFile, "Error.txt"),
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

    internal record AttachmentTemp(FileInfo File, string FileName);

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
