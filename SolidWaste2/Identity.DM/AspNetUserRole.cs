using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Identity.DM;

public class AspNetUserRole
{
    [Key]
    [Column(Order = 1)]
    [ForeignKey("User")]
    [StringLength(128)]
    public string UserId { get; set; }

    [Key]
    [Column(Order = 2)]
    [ForeignKey("Role")]
    [StringLength(128)]
    public string RoleId { get; set; }

    public AspNetRole Role { get; set; }

    public AspNetUser User { get; set; }
}
