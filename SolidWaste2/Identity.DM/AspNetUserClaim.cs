using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Identity.DM;

public class AspNetUserClaim
{
    [Key]
    public int Id { get; set; }

    [Required]
    [ForeignKey("User")]
    [StringLength(128)]
    public string UserId { get; set; }

    public string ClaimType { get; set; }

    public string ClaimValue { get; set; }

    public AspNetUser User { get; set; }
}
