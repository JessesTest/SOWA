using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SW.DM;

public class BillServiceAddress
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int BillServiceAddressId { get; set; }        
    
    public BillMaster BillMaster { get; set; }

    [ForeignKey("BillMaster"), Column(Order = 0)]
    public int BillMasterId { get; set; }

    public ServiceAddress ServiceAddress { get; set; }

    [ForeignKey("ServiceAddress"), Column(Order = 1)]
    public int ServiceAddressID { get; set; }

    [StringLength(100)]        
    public string ServiceAddressName { get; set; }

    [StringLength(100)]        
    public string ServiceAddressStreet { get; set; }

    [StringLength(100)]        
    public string ServiceAddressCityStateZip { get; set; }

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

    public virtual List<BillContainerDetail> BillContainerDetails { get; set; }
}
