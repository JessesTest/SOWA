using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SW.DM;

public class BillMaster
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int BillMasterId { get; set; }        
    
    public Customer Customer { get; set; }

    [ForeignKey("Customer"), Column(Order = 0)]
    public string CustomerType { get; set; }

    [ForeignKey("Customer"), Column(Order = 1)]
    public int CustomerID { get; set; }

    public Transaction Transaction { get; set; }
    
    [ForeignKey("Transaction")]
    public int TransactionID { get; set; }     

    public decimal? ContractCharge { get; set; }

    [StringLength(100)]
    public string BillingName { get; set; }

    [StringLength(100)]
    public string BillingAddressStreet { get; set; }

    [StringLength(100)]
    public string BillingAddressCityStateZip { get; set; }

    public DateTime BillingPeriodBegDate { get; set; }

    public DateTime BillingPeriodEndDate { get; set; }

    public decimal PastDueAmt { get; set; }

    public decimal ContainerCharges { get; set; }

    public bool FinalBill { get; set; }

    [StringLength(100)]        
    public string BillMessage { get; set; }

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

    public virtual List<BillServiceAddress> BillServiceAddresses { get; set; }
}
