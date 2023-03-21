﻿using System.ComponentModel.DataAnnotations;

namespace SW.ExternalWeb.Models.Account;

public class RegisterViewModel
{
    [Required]
    [Display(Name = "User Name")]
    public string UserName { get; set; }

    [Required]
    [EmailAddress]
    [Display(Name = "Email")]
    public string Email { get; set; }

    [Required]
    [Display(Name = "PIN")]
    public string Code { get; set; }

    [Required]
    //SCMB-248
    //[Display(Name = "Full Name")]
    [Display(Name = "Name on Bill")]
    public string FullName { get; set; }

    [Required]
    [StringLength(100, ErrorMessage = "The password must be at least {2} characters long.", MinimumLength = 8)]
    [DataType(DataType.Password)]
    [Display(Name = "Password")]
    public string Password { get; set; }

    [DataType(DataType.Password)]
    [Display(Name = "Confirm password")]
    [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
    public string ConfirmPassword { get; set; }
}
