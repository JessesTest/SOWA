using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Identity.DM;

public class AspNetUser
{
    [Key]
    [StringLength(128)]
    public string Id { get; set; }

    [StringLength(256)]
    public string Email { get; set; }

    public bool EmailConfirmed { get; set; }

    public string PasswordHash { get; set; }

    public string SecurityStamp { get; set; }

    public string PhoneNumber { get; set; }

    public bool PhoneNumberConfirmed { get; set; }

    public bool TwoFactorEnabled { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? LockoutEndDateUtc { get; set; }

    public bool LockoutEnabled { get; set; }

    public int AccessFailedCount { get; set; }

    [Required]
    [StringLength(256)]
    public string UserName { get; set; }

    public int UserId { get; set; }

    public string EmailConfirmationCode { get; set; }
    
    public virtual ICollection<AspNetUserClaim> AspNetUserClaims { get; set; }
    public virtual ICollection<AspNetUserLogin> AspNetUserLogins { get; set; }
    public virtual ICollection<AspNetRole> AspNetRoles { get; set; }
}
