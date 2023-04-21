using System.ComponentModel.DataAnnotations;

namespace SW.InternalWeb.Models.TransactionCode;

public class TransactionCodeAddViewModel
{
    [Required]
    [MaxLength(5)]
    [Display(Name = "Code")]
    [RegularExpression(@"^(([a-z]|[A-Z]){1,5})$", ErrorMessage = "Invalid Transaction Code.")]
    public string Code { get; set; }

    [Required]
    [MaxLength(50)]
    [Display(Name = "Code Description")]
    public string Description { get; set; }

    [Display(Name = "Current Balance Sign")]
    public string TransactionSign { get; set; }
    [Display(Name = "Collections Balance Sign")]
    public string CollectionsBalanceSign { get; set; }
    [Display(Name = "Counselors Balance Sign")]
    public string CounselorsBalanceSign { get; set; }
    [Display(Name = "Uncollectable Balance Sign")]
    public string UncollectableBalanceSign { get; set; }


    [Display(Name = "Acct Type")]
    public string AccountType { get; set; }

    //[Display(Name = "Group")]
    //public string Group { get; set; }
}
