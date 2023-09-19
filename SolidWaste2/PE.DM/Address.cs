using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PE.DM;


[Table("Address")]
public class Address
{
    public int Id { get; set; }

    [ForeignKey(nameof(Code))]
    public int Type { get; set; }

    public int PersonEntityID { get; set; }

    public int? Number { get; set; }

    [StringLength(255)]
    public string Direction { get; set; }

    [StringLength(255)]
    [Display(Name = "Street")]
    public string StreetName { get; set; }

    [StringLength(255)]
    public string Suffix { get; set; }

    [StringLength(255)]
    public string Apt { get; set; }

    [StringLength(255)]
    [Required]
    public string City { get; set; }

    [Required]
    [StringLength(255)]
    [RegularExpression(@"[a-zA-Z]{2}", ErrorMessage = "Invalid State")]
    public string State { get; set; }

    [StringLength(255)]
    [Required]
    [RegularExpression(@"[0-9]{5}(-[0-9]{4})?", ErrorMessage = "Invalid Zip")]
    public string Zip { get; set; }

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

    public virtual PersonEntity PersonEntity { get; set; }

    public virtual Code Code { get; set; }

    public bool IsDefault { get; set; }

    public bool Override { get; set; }

    //public Geography Location { get; set; }


    public override bool Equals(object obj)
    {
        return obj is Address other &&
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
