using Microsoft.AspNetCore.Mvc.Rendering;

namespace SW.InternalWeb.Models.Reports
{
    [Serializable]
    public class ReportsViewModel
    {
        public ContractChargeValueType ContractChargeValue { get; set; }
        public TransActionCodeType TransActionCode { get; set; }
        public DelinquentAccountType DelinquentAccount { get; set; }
        public WorkOrderType WorkOrder { get; set; }
        public KanPayActionCodeType KanPayActionCode { get; set; }
        public SWBatchBilling BatchBilling { get; set; }

        public ReportsViewModel()
        {
            ContractChargeValue = new ContractChargeValueType();
            TransActionCode = new TransActionCodeType();
            DelinquentAccount = new DelinquentAccountType();
            WorkOrder = new WorkOrderType();
            BatchBilling = new SWBatchBilling();
        }

        public class ContractChargeValueType
        {
            public bool SpreadSheet { get; set; }
            public bool ContractChargeZero { get; set; }
        }

        public class TransActionCodeType
        {
            public string Code { get; set; }
            public DateTime? StartDate { get; set; }
            public DateTime? EndDate { get; set; }
            public int? CustomerNumber { get; set; }
            public bool SpreadSheet { get; set; }
        }
        public MultiSelectList SelectedListCodes { get; set; }
        public List<SelectListItem> ListCodes { get; set; }

        public struct DelinquentAccountType
        {
            public string Account { get; set; }
            public DateTime? StartDate { get; set; }
            public DateTime? EndDate { get; set; }
            public int? CustomerNumber { get; set; }
            public bool SpreadSheet { get; set; }
        }
        public List<SelectListItem> ListAccounts { get; set; }

        public class WorkOrderType
        {
            public int? WorkOrderIdStart { get; set; }
            public int? WorkOrderIdEnd { get; set; }
            public string TransDateStart { get; set; }
            public string TransDateEnd { get; set; }
            public string DriverInitials { get; set; }
            public string CustomerType { get; set; }
            public string ContainerRouteStart { get; set; }
            public string ContainerRouteEnd { get; set; }
            public bool IncludeResolved { get; set; }
        }

        public class KanPayActionCodeType
        {
            public string PayType { get; set; }
            public DateTime? StartDate { get; set; }
            public DateTime? EndDate { get; set; }
            public int? CustomerNumber { get; set; }
            public bool SpreadSheet { get; set; }
        }
        public MultiSelectList KanPaySelectedListPayTypes { get; set; }
        public List<SelectListItem> KanPayListPayTypes { get; set; }

        public class SWBatchBilling
        {
            public string BegDateTime { get; set; }
            public string EndDateTime { get; set; }
            public string CustomerId { get; set; }
        }
    }
}
