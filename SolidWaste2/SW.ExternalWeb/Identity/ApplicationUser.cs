using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

namespace SW.ExternalWeb.Identity
{
    public class ApplicationUser : IdentityUser
    {
        public int UserId { get; set; }
        public string EmailConfirmationCode { get; set; }
        public string PasswordResetCode { get; set; }

        public DateTime? LockoutEndDateUtc { get; set; }    // old identity
    }
}
