using Microsoft.AspNetCore.Mvc.Rendering;
using SW.InternalWeb.Extensions;
using System.ComponentModel.DataAnnotations;

namespace SW.InternalWeb.Models.CustomerEmail;

public class CustomerEmailViewModel
{
    public string CustomerType { get; set; }

    public int CustomerID { get; set; }

    public string FullName { get; set; }

    public int? Id { get; set; }

    public int Type { get; set; }

    public int PaperLess { get; set; }

    //[RegularExpression(@"^(?("")("".+?(?<!\\)""@)|(([0-9a-z]((\.(?!\.))|[-!#\$%&'\*\+/=\?\^`\{\}\|~\w])*)(?<=[0-9a-z])@))" +
    //        @"(?(\[)(\[(\d{1,3}\.){3}\d{1,3}\])|(([0-9a-z][-\w]*[0-9a-z]*\.)+[a-z0-9][\-a-z0-9]{0,22}[a-z0-9]))$",
    //         ErrorMessage = "Invalid Email")]
    //[RegularExpression(@"[a-z0-9!#$%&'*+/=?^_`{|}~-]+(?:\.[a-z0-9!#$%&'*+/=?^_`{|}~-]+)*@(?:[a-z0-9](?:[a-z0-9-]*[a-z0-9])?\.)+[a-z0-9](?:[a-z0-9-]*[a-z0-9])?", ErrorMessage = "Invalid Email")]
    [Display(Name = "Email Address")]
    [StringLength(255)]
    [EmailAddress]
    public string Email1 { get; set; }

    [Required]
    public bool Status { get; set; }

    public int CurrentIndex { get; set; }

    public int MaxIndex { get; set; }
}
