using Microsoft.AspNetCore.Mvc.Rendering;
using PE.BL.Services;
using SW.ExternalWeb.Controllers;

namespace SW.ExternalWeb.Extensions
{
    public class Helpers
    {
        private readonly IEmailService _emailService;
        private readonly IPhoneService _phoneService;

        public Helpers(IEmailService emailService, IPhoneService phoneService)
        {
            _emailService = emailService;
            _phoneService = phoneService;
        }

        public async Task<IEnumerable<SelectListItem>> GenerateEmailsSelectList(int peID)
        {
            var emails = await _emailService.GetByPerson(peID, false);

            return emails.Select(e => new SelectListItem { Text = e.Email1, Value = e.Id.ToString() });
        }

        public async Task<IEnumerable<SelectListItem>> GeneratePhonesSelectList(int peID)
        {
            var phones = await _phoneService.GetByPerson(peID, false);

            var selectList = phones.Select(p => new SelectListItem 
            { 
                Text = string.Format("{0}{1}", p.PhoneNumber, p.Ext == null ? string.Empty : " x" + p.Ext), 
                Value = p.Id.ToString() 
            }).ToList();
            selectList.Insert(0, new SelectListItem { Text = "None", Value = "0" });

            return selectList;
        }
    }
}
