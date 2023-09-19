using System.ComponentModel.DataAnnotations.Schema;

namespace SW.DM;

public class TransactionCode
{
    public TransactionCode()
    {
        TransactionCodeRules = new HashSet<TransactionCodeRule>();
        TransactionHoldings = new HashSet<TransactionHolding>();
        Transactions = new HashSet<Transaction>();
    }

    public int TransactionCodeId { get; set; }
    public string Code { get; set; }
    public string Description { get; set; }
    public string TransactionSign { get; set; }
    public string CollectionsBalanceSign { get; set; }
    public string CounselorsBalanceSign { get; set; }
    public string AccountType { get; set; }
    public string Group { get; set; }
    public bool DeleteFlag { get; set; }
    public DateTime AddDateTime { get; set; }
    public string AddToi { get; set; }
    public DateTime? ChgDateTime { get; set; }
    public string ChgToi { get; set; }
    public DateTime? DelDateTime { get; set; }
    public string DelToi { get; set; }
    public string Security { get; set; }
    public bool Hold { get; set; }
    public string UncollectableBalanceSign { get; set; }

    public virtual ICollection<TransactionCodeRule> TransactionCodeRules { get; set; }
    public virtual ICollection<TransactionHolding> TransactionHoldings { get; set; }
    public virtual ICollection<Transaction> Transactions { get; set; }




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
        newTran.TransactionCodeId = TransactionCodeId;

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
