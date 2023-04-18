using SW.BLL.Services;
using System.ComponentModel.DataAnnotations;

namespace SW.InternalWeb.Models.CustomerInquiry;

public class CustomerInquiryParametersViewModel
{
    [Display(Name = "Customer Name")]
    public string CustomerName { get; set; }

    [Display(Name = "Customer Number")]
    public int? CustomerNumber { get; set; }

    [Display(Name = "PIN")]
    public string PIN { get; set; }

    [Display(Name = "Billing Address")]
    public string CustomerAddress { get; set; }

    [Display(Name = "Location Number")]
    public string LocationNumber { get; set; }

    [Display(Name = "Location Name")]
    public string LocationName { get; set; }

    [Display(Name = "Service Address")]
    public string LocationAddress { get; set; }

    public bool Include { get; set; }

    public string Command { get; set; }

    public IEnumerable<CustomerInquiryResult> Results { get; set; }
}
