using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SW.DM;

public class Formula
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int FormulaID { get; set; }
    
    [StringLength(255)]
    public string Name { get; set; }

    [StringLength(255)]
    public string FormulaString { get; set; }

    [StringLength(255)]
    public string CommentString { get; set; }

    public bool Delete { get; set; }

    public DateTime AddDateTime { get; set; }

    public DateTime? ChgDateTime { get; set; }

    public DateTime? DelDateTime { get; set; }

    [StringLength(255)]
    public string AddToi { get; set; }

    [StringLength(255)]
    public string ChgToi { get; set; }

    [StringLength(255)]
    public string DelToi { get; set; }

    public virtual ICollection<Parameter> Parameters { get; set; }
}
