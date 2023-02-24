using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SW.DM;

public class Parameter
{
    [Key]
    [Column(Order = 0)]
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public int FormulaID { get; set; }

    [Key]
    [Column(Order = 1)]
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public string ParameterID { get; set; }

    [StringLength(255)]
    public string Name { get; set; }

    public decimal? Value { get; set; }

    public bool Constant { get; set; }

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
        
    [ForeignKey("FormulaID")]
    public virtual Formula Formula { get; set; }
}
