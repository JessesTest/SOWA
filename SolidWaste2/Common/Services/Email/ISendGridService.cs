namespace Common.Services.Email;

public interface ISendGridService
{
    Task SendSingleEmail(SendEmailDto dto);
    Task SendMultipleEmails(SendEmailDto dto);
}
