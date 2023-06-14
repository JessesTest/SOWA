namespace SW.InternalWeb.Models.TransactionApi;

public class PaymentPlanJson
{
    public IEnumerable<DetailJson> Details { get; set; }

    public class DetailJson
    {
        public string DueDate { get; set; }
        public decimal Amount { get; set; }
        public decimal PaymentTotal { get; set; }
        public bool Paid { get; set; }
    }
}
