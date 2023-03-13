namespace Identity.BL.Services
{
    public interface IUserNotificationService
    {
        Task SendConfirmationEmailByUserId(int userId, string callbackUrl);
        Task SendEmail2FA(string to, string code);
    }
}