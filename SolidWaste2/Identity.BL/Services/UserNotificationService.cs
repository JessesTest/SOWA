using Common.Services.Email;
using Identity.DAL.Contexts;
using Microsoft.EntityFrameworkCore;

namespace Identity.BL.Services
{
	public class UserNotificationService : IUserNotificationService
    {
        private readonly IDbContextFactory<IdentityDbContext> contextFactory;
        private readonly ISendGridService emailService;

        private readonly string from = "no-reply.scsw@sncoapps.us";

        public UserNotificationService(
            IDbContextFactory<IdentityDbContext> contextFactory,
            ISendGridService emailService)
        {
            this.contextFactory = contextFactory;
            this.emailService = emailService;
        }

        #region Confirm email

        public async Task SendConfirmationEmailByUserId(int userId, string callbackUrl)
        {
            using var db = contextFactory.CreateDbContext();
            var user = await db.AspNetUsers
                .Where(u => u.UserId == userId)
                .SingleOrDefaultAsync();

            if (string.IsNullOrWhiteSpace(user.EmailConfirmationCode))
            {
                var guid = Guid.NewGuid();
                user.EmailConfirmationCode = guid.ToString();

                await db.SaveChangesAsync();
            }

            var code = user.EmailConfirmationCode;
            var url = $"{callbackUrl}?userId={user.UserId}&code={code}";

            var emailDto = new SendEmailDto
            {
                HtmlContent = GenerateConfirmEmailBody(url),
                Subject = "Please confirm your email with Shawnee County Solid Waste"
            }
            .SetFrom(from)
            .AddTo(user.Email);

            await emailService.SendSingleEmail(emailDto);
        }
        private static string GenerateConfirmEmailBody(string url)
        {
            return $@"
<div style='background-color:#909EB8'>
	<div align='center'>
		<div style='background-color:#344479; min-height:100px; padding-left: 20px; padding-right: 20px; padding-top: 28px; height: 100px; min-width: 100%; display:table; overflow:hidden;'>
			<div style='display: table-cell; vertical-align: middle; font-size: 40px; color:white; font-family: Verdana;'>
				<div align='center'>
					Shawnee County Solid Waste
				</div>
			</div>
		</div>
	</div>
	<div align='center'>
		<div style='padding: 20px 20px 20px 20px; background-color: #D4D4D4; width: 50%;'>
			<div style='padding: 20px 20px 20px 20px; background-color: #FFFFFF;'>
				<div style='display: block; margin-bottom: 20px;'>
					Please confirm your email address by clicking the link below:
				</div>
				<div style='display: block; margin-bottom: 20px; text-align: center;'>
					<div align='center'>
						<a style='padding: 8px 15px 8px 15px; font-size:14px; text-align:center; background-color: #6EBF4A; color: #FFFFFF; font-weight: bold; text-decoration: none;' href='{url}'>Confirm Email</a>
					</div>
				</div>
			</div>
			<div align='center' style='padding-top:20px'>
				<div style='font-size:12px;'>
					DO NOT REPLY TO THIS EMAIL.  If you wish to stop receiving email notices, you may update your email preference from your online profile. <br> If you've received this email in error, please notify us by telephone at 785.233.4774 or by email at solidwaste@snco.us.
				</div>
			</div>
			<hr>
			<div width='50%'>
				Visit the Shawnee County Website at http://www.snco.us
			</div>
			<div width='50%'>
				Questions? Contact Solid Waste at 785.233.4774 (voice) 785.291.4928(fax)
			</div>
		</div>
	</div>
</div>
";
        }

        #endregion

        #region 2FA email

        public Task SendEmail2FA(string to, string code)
        {
            var subject = "Shawnee County Solid Waste Login";
            var text = code;

            var email = new SendEmailDto
            {
                TextContent = text,
                Subject = subject
            }
            .SetFrom(from)
            .AddTo(to);

            return emailService.SendSingleEmail(email);
        }

        #endregion

    }
}
