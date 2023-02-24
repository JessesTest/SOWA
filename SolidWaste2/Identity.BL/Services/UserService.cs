using Identity.DAL.Contexts;
using Identity.DM;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Identity.BL.Services
{
    public class UserService : IUserService
    {
        private readonly IDbContextFactory<IdentityDbContext> contextFactory;

        public UserService(IDbContextFactory<IdentityDbContext> contextFactory)
        {
            this.contextFactory = contextFactory;
        }

        public Task<AspNetUser> GeByUserName(string userName)
        {
            throw new NotImplementedException();
        }

        public Task<AspNetUser> GetByUserId(int userId)
        {
            throw new NotImplementedException();
        }

        public Task<bool> UserNameExists(string userName)
        {
            throw new NotImplementedException();
        }

        public Task Update(AspNetUser user)
        {
            throw new NotImplementedException();
        }

        public Task DeleteByUserId(int userId)
        {
            throw new NotImplementedException();
        }

        public Task DeleteByUserName(string userName)
        {
            throw new NotImplementedException();
        }

        public Task Delete(AspNetUser user)
        {
            throw new NotImplementedException();
        }

        public Task<AspNetUser> GetByEmail(string email)
        {
            throw new NotImplementedException();
        }

        public Task SendConfirmationEmailByUserId(int userId, string callbackUrl)
        {
            throw new NotImplementedException();
        }

        public Task SendConfirmationEmail(AspNetUser user, string callbackUrl)
        {
            throw new NotImplementedException();
        }

        public Task<bool> ConfirmEmail(int userId, string code)
        {
            throw new NotImplementedException();
        }

        private static string GenerateConfirmEmailBody(string url)
        {
            var sb = new StringBuilder();
            sb.AppendLine("<div style=\"background-color:#909EB8\">");
            sb.AppendLine("     <div align=\"center\">");
            sb.AppendLine("         <div style=\"background-color:#344479; min-height:100px; padding-left: 20px; padding-right: 20px; padding-top: 28px; height: 100px; min-width: 100%; display:table; overflow:hidden;\">");
            sb.AppendLine("             <div style=\"display: table-cell; vertical-align: middle; font-size: 40px; color:white; font-family: Verdana;\">");
            sb.AppendLine("                 <div align=\"center\">");
            sb.AppendLine("                     Shawnee County Solid Waste");
            sb.AppendLine("                 </div>");
            sb.AppendLine("             </div>");
            sb.AppendLine("         </div>");
            sb.AppendLine("     </div>");
            sb.AppendLine("     <div align=\"center\">");
            sb.AppendLine("        <div style=\"padding: 20px 20px 20px 20px; background-color: #D4D4D4; width: 50%;\">");
            sb.AppendLine("            <div style=\"padding: 20px 20px 20px 20px; background-color: #FFFFFF;\">");
            sb.AppendLine("                <div style=\"display: block; margin-bottom: 20px;\">");
            sb.AppendLine("                   Please confirm your email address by clicking the link below:");
            sb.AppendLine("                </div>");
            sb.AppendLine("                <div style=\"display: block; margin-bottom: 20px; text-align: center;\">");
            sb.AppendLine("                     <div align=\"center\">");
            sb.AppendLine($"                        <a style=\"padding: 8px 15px 8px 15px; font-size:14px; text-align:center; background-color: #6EBF4A; color: #FFFFFF; font-weight: bold; text-decoration: none;\" href=\"{url}\">Confirm Email</a>");
            sb.AppendLine("                    </div>");
            sb.AppendLine("                 </div>");
            sb.AppendLine("            </div>");
            sb.AppendLine("         <div align=\"center\" style=\"padding-top:20px\">");
            sb.AppendLine("             <div style=\"font-size:12px;\">");
            sb.AppendLine("                 DO NOT REPLY TO THIS EMAIL.  If you wish to stop receiving email notices, you may update your email preference from your online profile. <br> If you've received this email in error, please notify us by telephone at 785.233.4774 or by email at solidwaste@snco.us.");
            sb.AppendLine("         </div>");
            sb.AppendLine("     </div>");
            sb.AppendLine("<hr>");
            sb.AppendLine("     <div width=\"50%\">");
            sb.AppendLine("         Visit the Shawnee County Website at http://www.snco.us");
            sb.AppendLine("     </div>");
            sb.AppendLine("     <div width=\"50%\">");
            sb.AppendLine("         Questions? Contact Solid Waste at 785.233.4774 (voice) 785.291.4928(fax)");
            sb.AppendLine("     </div>");
            sb.AppendLine("</div>");
            sb.AppendLine("</div>");

            return sb.ToString();
        }
    }
}
