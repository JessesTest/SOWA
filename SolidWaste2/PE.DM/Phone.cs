using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PE.DM;

[Table("Phone")]
public class Phone
{
    public int Id { get; set; }

    [ForeignKey(nameof(Code))]
    public int Type { get; set; }

    public int PersonEntityID { get; set; }

    [StringLength(255)]
    [Required]
    [RegularExpression(@"^\(\d{3}\) \d{3}-\d{4}$", ErrorMessage = "Invalid Phone Number")]
    public string PhoneNumber { get; set; }

    [StringLength(255)]
    [RegularExpression(@"\d+")]
    public string Ext { get; set; }

    public bool Status { get; set; }

    public bool Delete { get; set; }

    public DateTime? AddDateTime { get; set; }

    [StringLength(50)]
    public string AddToi { get; set; }

    public DateTime? ChgDateTime { get; set; }

    [StringLength(50)]
    public string ChgToi { get; set; }

    public DateTime? DelDateTime { get; set; }

    [StringLength(50)]
    public string DelToi { get; set; }

    public virtual Code Code { get; set; }

    public virtual PersonEntity PersonEntity { get; set; }

    public bool IsDefault { get; set; }


    public override bool Equals(object obj)
    {
        return obj is Phone other &&
            Id > 0 &&
            Id == other.Id;
    }

    public override int GetHashCode()
    {
        HashCode hash = new();
        hash.Add(Id);
        return hash.ToHashCode();
    }
}
