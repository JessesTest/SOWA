using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SW.DM;

public class ServiceAddress
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [ForeignKey("Customer"), Column(Order = 0)]
    public string CustomerType { get; set; }
    [ForeignKey("Customer"), Column(Order = 1)]
    public int CustomerId { get; set; }
    public Customer Customer { get; set; }

    // not used
    [StringLength(50)]
    public string ServiceType { get; set; }

    [StringLength(50)]
    public string LocationNumber { get; set; }

    [StringLength(50)]
    public string LocationName { get; set; }

    [StringLength(50)]
    public string LocationContact { get; set; }

    public DateTime EffectiveDate { get; set; }

    public DateTime? CancelDate { get; set; }

    public int PEAddressId { get; set; }

    [StringLength(100)]
    public string Email { get; set; }

    [StringLength(20)]
    public string Phone { get; set; }

    public virtual IList<ServiceAddressNote> Notes { get; set; }

    public virtual IList<Container> Containers { get; set; }

    public virtual IList<BillServiceAddress> Bills { get; set; }

    public bool DeleteFlag { get; set; }

    public DateTime AddDateTime { get; set; }

    [StringLength(255)]
    public string AddToi { get; set; }

    public DateTime? ChgDateTime { get; set; }

    [StringLength(255)]
    public string ChgToi { get; set; }

    public DateTime? DelDateTime { get; set; }

    [StringLength(255)]
    public string DelToi { get; set; }

    [NotMapped]
    public PE.DM.Address PEAddress { get; set; }
}
