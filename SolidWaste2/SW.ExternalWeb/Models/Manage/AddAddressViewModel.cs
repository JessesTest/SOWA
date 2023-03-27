using Common.Services.AddressValidation;
using Microsoft.AspNetCore.Mvc.Rendering;
using SW.ExternalWeb.Extensions;
using System.ComponentModel.DataAnnotations;

namespace SW.ExternalWeb.Models.Manage
{
    public class AddAddressViewModel
    {
        public IList<AddressListViewModel> Addresses { get; set; }

        public IList<PE.DM.Address> ValidAddresses { get; set; }

        public int? ValidAddressSelect { get; set; }

        [Display(Name = "Street Address")]
        [Required]
        public string StreetAddress { get; set; }

        [Display(Name = "City")]
        public string City { get; set; }

        [Display(Name = "State")]
        public string State { get; set; }

        [Display(Name = "Zip")]
        public string Zip { get; set; }

        public bool MakePrimary { get; set; }

        public bool Override { get; set; }

        public bool ValidationMode { get; set; }

        public IList<SelectListItem> ValidAddressesSelectList
        {
            get
            {
                var selectList = new List<SelectListItem>();
                if (ValidAddresses == null)
                {
                    return selectList;
                }
                for (int i = 0; i < ValidAddresses.Count(); i++)
                {
                    var va = ValidAddresses.ElementAt(i);
                    selectList.Add(new SelectListItem() { Value = i.ToString(), Text = va.ToLine1String() });
                }

                return selectList;
            }
        }
    }
}
