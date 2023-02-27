using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SW.DM;

public class TransactionCode
{
    [Key]
    [Column(Order = 0)]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int TransactionCodeID { get; set; }

    [StringLength(5)]
    public string Code { get; set; }

    [StringLength(50)]
    public string Description { get; set; }

    [StringLength(8)]
    public string TransactionSign { get; set; }
    [StringLength(8)]
    public string CollectionsBalanceSign { get; set; }
    [StringLength(8)]
    public string CounselorsBalanceSign { get; set; }
    [StringLength(8)]
    public string UncollectableBalanceSign { get; set; }

    [StringLength(8)]
    public string AccountType { get; set; }

    [StringLength(8)]
    public string Group { get; set; }

    [StringLength(32)]
    public string Security { get; set; }

    public bool Hold { get; set; }

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

    protected decimal ProcessSignAmount(string sign, decimal amount)
    {
        if (!string.IsNullOrWhiteSpace(sign))
        {
            if (sign == "P")
                return amount < 0 ? -amount : amount;
            if (sign == "N")
                return amount > 0 ? -amount : amount;
            if (sign == "B")
                return amount;
        }
        return 0;
    }

    public void Process(decimal amount, Transaction oldTran, Transaction newTran)
    {
        newTran.TransactionCodeId = TransactionCodeID;

        newTran.CollectionsAmount = ProcessSignAmount(CollectionsBalanceSign, amount);
        newTran.CollectionsBalance = oldTran.CollectionsBalance + newTran.CollectionsAmount;

        newTran.CounselorsAmount = ProcessSignAmount(CounselorsBalanceSign, amount);
        newTran.CounselorsBalance = oldTran.CounselorsBalance + newTran.CounselorsAmount;

        newTran.TransactionAmt = ProcessSignAmount(TransactionSign, amount);
        newTran.TransactionBalance = oldTran.TransactionBalance + newTran.TransactionAmt;

        newTran.UncollectableAmount = ProcessSignAmount(UncollectableBalanceSign, amount);
        newTran.UncollectableBalance = oldTran.UncollectableBalance.GetValueOrDefault() + newTran.UncollectableAmount.Value;
    }

    [NotMapped]
    public bool IsCollections
    {
        get
        {
            return ProcessSignAmount(CollectionsBalanceSign, 1) != 0;
        }
    }

    [NotMapped]
    public bool IsCounselors
    {
        get
        {
            return ProcessSignAmount(CounselorsBalanceSign, 1) != 0;
        }
    }

    [NotMapped]
    public bool IsUncollectable
    {
        get
        {
            return ProcessSignAmount(UncollectableBalanceSign, 1) != 0;
        }
    }

    [NotMapped]
    public bool IsTransaction
    {
        get
        {
            return ProcessSignAmount(TransactionSign, 1) != 0;
        }
    }
}
