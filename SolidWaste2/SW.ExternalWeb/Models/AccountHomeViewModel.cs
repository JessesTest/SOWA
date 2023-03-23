using System.ComponentModel.DataAnnotations;

namespace SW.ExternalWeb.Models;

public class AccountHomeViewModel
{
    public string CustomerId { get; set; }

    [Display(Name = "Billing Address")]
    public string BillingAddress { get; set; }

    public string CurrentBillTransactionId { get; set; }

    public DateTime? AccountSummaryPeriodBeginDate { get; set; }

    public DateTime? AccountSummaryPeriodEndDate { get; set; }

    public DateTime AccountSummaryDueDate { get; set; }

    public decimal AccountSummaryPreviousBalance { get; set; }

    public decimal AccountSummaryAnyPayments { get; set; }

    public decimal AccountSummaryAnyAdjustments { get; set; }

    public decimal AccountSummaryNewCharges { get; set; }

    public decimal AccountSummaryTotalDue { get; set; }
}

public class AccountHomeTransactionListViewModel
{
    public string AddDateTime { get; set; }

    public string Activity { get; set; }

    public string TransactionAmount { get; set; }

    public string Balance { get; set; }

    public string Address { get; set; }

    public string Container { get; set; }
}
public class CustomerTransactionHistoryViewModel
{
    public List<CustomerTransactionViewModel> Transactions { get; set; }

    public int CustomerID { get; set; }
}

public class CustomerTransactionViewModel
{
    public CustomerBillMasterViewModel BillMaster { get; set; }

    public string Activity { get; set; }

    public string Balance { get; set; }

    public string Container { get; set; }
    public string AddDateTime { get; set; }
    public string Description { get; set; }
    public string UserID { get; set; }
    public string BatchID { get; set; }
    public string CheckNumber { get; set; }
    public string TransactionAmount { get; set; }
    public string CounselorsAmount { get; set; }
    public string CollectionsAmount { get; set; }
    public string TransactionBalance { get; set; }
    public string CounselorsBalance { get; set; }
    public string CollectionsBalance { get; set; }
    public string Comments { get; set; }
    public string WorkOrder { get; set; }
    public string Sequence { get; set; }
    public string ContractCharge { get; set; }

}
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
public class CustomerBillServiceAddressViewModel
{
    public List<CustomerBillContainerDetailViewModel> BillContainers { get; set; }
    public string BillMasterId { get; set; }
    public string BillServiceAddressId { get; set; }
    public string ServiceAddressName { get; set; }
    public string ServiceAddressStreet { get; set; }

}
public class CustomerBillContainerDetailViewModel
{
    public string BillServiceAddressId { get; set; }
    public string BillContainerDetailId { get; set; }
    public string ContainerType { get; set; }
    public string ContainerDescription { get; set; }
    public string ContainerEffectiveDate { get; set; }
    public string ContainerCancelDate { get; set; }
    public string RateAmount { get; set; }
    public string RateDescription { get; set; }
    public string DaysProratedMessage { get; set; }
    public string DaysService { get; set; }
    public string ContainerCharge { get; set; }
}