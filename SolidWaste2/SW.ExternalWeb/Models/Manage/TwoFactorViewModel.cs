using Microsoft.AspNetCore.Mvc.Rendering;
using SW.ExternalWeb.Extensions;

namespace SW.ExternalWeb.Models.Manage
{
    public class TwoFactorViewModel
    {
        public IEnumerable<SelectListItem> EmailsDropDown { get; set; }
        public IEnumerable<SelectListItem> PhonesDropDown { get; set; }

        public int TwoFactorEmailID { get; set; }
        public int TwoFactorPhoneID { get; set; }

        public bool TwoFactor { get; set; }
        public bool BrowserRemembered { get; set; }
    }
}
