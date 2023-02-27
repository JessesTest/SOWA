using Identity.DM;

namespace Identity.BL.Services;

public interface IUserService
{
    Task<bool> ConfirmEmail(int userId, string code);
    Task Delete(AspNetUser user);
    Task DeleteByUserId(int userId);
    Task DeleteByUserName(string userName);
    Task<AspNetUser> GeByUserName(string userName);
    Task<AspNetUser> GetByEmail(string email);
    Task<AspNetUser> GetByUserId(int userId);
    Task SendConfirmationEmail(AspNetUser user, string callbackUrl);
    Task SendConfirmationEmailByUserId(int userId, string callbackUrl);
    Task Update(AspNetUser user);
    Task<bool> UserNameExists(string userName);
}
