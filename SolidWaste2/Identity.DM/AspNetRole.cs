using System.ComponentModel.DataAnnotations;

namespace Identity.DM;

public class AspNetRole
{
    [Key]
    [StringLength(128)]
    public string Id { get; set; }

    [Required]
    [StringLength(256)]
    public string Name { get; set; }
    
    public virtual ICollection<AspNetUser> AspNetUsers { get; set; }
}
