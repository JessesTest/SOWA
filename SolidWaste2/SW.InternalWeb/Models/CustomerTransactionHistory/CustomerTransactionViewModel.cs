namespace SW.InternalWeb.Models.CustomerTransactionHistory;

public class CustomerTransactionViewModel
{
    public CustomerBillMasterViewModel BillMaster { get; set; }
    public string AddDateTime { get; set; }
    public string Description { get; set; }
    public string UserID { get; set; }
    public string BatchID { get; set; }
    public string CheckNumber { get; set; }
    public string TransactionAmount { get; set; }
    public string CounselorsAmount { get; set; }
    public string CollectionsAmount { get; set; }
    public string UncollectableAmount { get; set; }
    public string TransactionBalance { get; set; }
    public string CounselorsBalance { get; set; }
    public string CollectionsBalance { get; set; }
    public string UncollectableBalance { get; set; }
    public string Comments { get; set; }
    public string WorkOrder { get; set; }
    public string Sequence { get; set; }
    public string ContractCharge { get; set; }

}
