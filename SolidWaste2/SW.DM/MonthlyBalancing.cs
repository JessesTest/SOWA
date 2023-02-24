using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SW.DM;

public class MonthlyBalancing
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int MonthlyBalancingID { get; set; }

    public int Year { get; set; }

    public int Month {get; set;}

    public int ObjectCode { get; set; }

    [StringLength(32)]
    public string ObjectDescription { get; set; }

    public decimal BeginningBalance { get; set; }

    public decimal AccountTypeRevenue { get; set; }

    public decimal AccountTypeReceivable { get; set; }

    public decimal TotalRevenue { get; set; }

    public decimal TotalReceivable { get; set; }

    [StringLength(16)]
    public string AccountType { get; set; }

    [StringLength(4)]
    public string TransactionCode { get; set; }

    [StringLength(64)]
    public string TransactionCodeDescription { get; set; }

    public decimal TransactionAmount { get; set; }

    public bool DeleteFlag { get; set; }

    public DateTime? AddDateTime { get; set; }
    public DateTime? ChgDateTime { get; set; }
    public DateTime? DelDateTime { get; set; }

    public string AddToi { get; set; }
    public string ChgToi { get; set; }
    public string DelToi { get; set; }
}
