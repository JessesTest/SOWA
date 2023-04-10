using SW.BLL.Services;
using System.ComponentModel.DataAnnotations;

namespace SW.InternalWeb.Models.Customer;

public class CustomerViewModel
{
    //public IEnumerable<SelectListItem> CustomerTypes { get; set; }

    [Required]
    [Display(Name = "Customer Type")]
    public string CustomerType { get; set; }

    [Display(Name = "Number")]
    public int? CustomerID { get; set; }

    [Display(Name = "Customer Name Attn")]
    [StringLength(64)]
    public string NameAttn { get; set; }

    [Display(Name = "Customer Contact")]
    [StringLength(64)]
    public string Contact { get; set; }

    [Required]
    [Display(Name = "Effective Date")]
    public DateTime? EffectiveDate { get; set; }

    [Display(Name = "Cancel Date")]
    public DateTime? CancelDate { get; set; }

    [RegularExpression(@"^\-?\(?\$?\s*\-?\s*\(?(((\d{1,3}((\,\d{3})*|\d*))?(\.\d{1,4})?)|((\d{1,3}((\,\d{3})*|\d*))(\.\d{0,4})?))\)?$", ErrorMessage = "Invalid Contract Charge")]
    [Display(Name = "Contract Charge")]
    public string ContractCharge { get; set; }

    [Display(Name = "Purchase Order")]
    [StringLength(16)]
    public string PurchaseOrder { get; set; }

    [Display(Name = "Customer Notes")]
    [StringLength(255)]
    public string Notes { get; set; }

    [Display(Name = "Total Due")]
    public decimal? CurrentBalance { get; set; }

    [Required]
    [Display(Name = "Customer Name")]
    [StringLength(64)]
    public string FullName { get; set; }

    public bool NameTypeFlag { get; set; }

    //public CustomerViewModel()
    //{
    //    CustomerTypes = Helpers.GenerateCustomerCodeSelectList();
    //}

    public decimal PastDueAmount { get; set; }

    public decimal PastDue30Days { get; set; }
    public decimal PastDue60Days { get; set; }
    public decimal PastDue90Days { get; set; }
    public decimal CollectionsBalance { get; set; }
    public decimal CounselorsBalance { get; set; }
    public decimal UncollectableBalance { get; set; }

    [Display(Name = "PIN")]
    public string Account { get; set; }

    [Display(Name = "Legacy ID")]
    public int? LegacyCustomerID { get; set; }


    public ICollection<MieDataInfo> ActiveImages { get; set; }
    public ICollection<MieDataInfo> InactiveImages { get; set; }
}
