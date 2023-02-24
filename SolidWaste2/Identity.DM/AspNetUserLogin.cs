using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Identity.DM;

public class AspNetUserLogin
{
    [Key]
    [Column(Order = 1)]
    [StringLength(128)]
    public string LoginProvider { get; set; }

    [Key]
    [Column(Order = 2)]
    [StringLength(128)]
    public string ProviderKey { get; set; }

    [Key]
    [Column(Order = 3)]
    [ForeignKey("User")]
    [StringLength(128)]
    public string UserId { get; set; }

    public AspNetUser User { get; set; }
}
