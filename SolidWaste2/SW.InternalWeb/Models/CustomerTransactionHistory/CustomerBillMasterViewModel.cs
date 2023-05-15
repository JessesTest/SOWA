namespace SW.InternalWeb.Models.CustomerTransactionHistory;

public class CustomerBillMasterViewModel
{
    public List<CustomerBillServiceAddressViewModel> BillServiceAddress { get; set; }
    public string CustomerType { get; set; }
    public string CustomerID { get; set; }
    public string BillMasterId { get; set; }
    public string TransactionID { get; set; }
    public string ContractCharge { get; set; }
    public string BilllingName { get; set; }
    public string BillingAddressStreet { get; set; }
    public string BillingAddressCityStateZip { get; set; }
    public string BillingPeriodBegDate { get; set; }
    public string BillingPeriodEndDate { get; set; }
    public string PastDueAmt { get; set; }
    public string ContainerCharges { get; set; }
    public string FinalBill { get; set; }
    public string BillMessage { get; set; }
    public string DeleteFlag { get; set; }
    public string AddDateTime { get; set; }
    public string AddToi { get; set; }
    public string ChgDateTime { get; set; }
    public string ChgToi { get; set; }
    public string DelDateTime { get; set; }
    public string DelToi { get; set; }
}
