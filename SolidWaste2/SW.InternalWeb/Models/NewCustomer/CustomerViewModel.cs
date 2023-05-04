using System.ComponentModel.DataAnnotations;

namespace SW.InternalWeb.Models.NewCustomer;

public class CustomerViewModel
{
    [Required]
    [Display(Name = "Customer Type")]
    public string CustomerType { get; set; }

    [Display(Name = "Customer Name Attn")]
    [StringLength(64)]
    public string NameAttn { get; set; }

    [Display(Name = "Customer Contact")]
    [StringLength(64)]
    public string Contact { get; set; }

    [Required]
    [Display(Name = "Effective Date")]
    public DateTime? EffectiveDate { get; set; }

    [RegularExpression(@"^\-?\(?\$?\s*\-?\s*\(?(((\d{1,3}((\,\d{3})*|\d*))?(\.\d{1,4})?)|((\d{1,3}((\,\d{3})*|\d*))(\.\d{0,4})?))\)?$", ErrorMessage = "Invalid Contract Charge")]
    [Display(Name = "Contract Charge")]
    public string ContractCharge { get; set; }


    [Display(Name = "Purchase Order")]
    [StringLength(16)]
    public string PurchaseOrder { get; set; }

    [Display(Name = "Customer Notes")]
    [StringLength(255)]
    public string Notes { get; set; }

    [Required]
    [Display(Name = "Customer Name")]
    [StringLength(64)]
    public string FullName { get; set; }

    public bool NameTypeFlag { get; set; }


    [Display(Name = "PIN")]
    public string Account { get; set; }
}
