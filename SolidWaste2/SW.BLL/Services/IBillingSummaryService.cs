using PE.DM;
using SW.DM;

namespace SW.BLL.Services;

public interface IBillingSummaryService
{
    Task<decimal> GetTotalAdjustmentsForCustomerInDateRange(int customerId, DateTime startDate, DateTime endDate);
    Task<decimal> GetTotalNewChargesForCustomerInDateRange(int customerId, DateTime startDate, DateTime endDate);
    Task<decimal> GetTotalPaymentsForCustomerInDateRange(int customerId, DateTime startDate, DateTime endDate);

    Task<BillingSummary> GetBillingSummary(int customerId);
}

public class BillingSummary : List<BillingSummaryServiceAddress>
{
    public decimal Total => IsContractCharge ? ContractCharge : MonthlyTotal;
    public bool IsContractCharge { get; set; }
    public decimal ContractCharge { get; set; }
    public decimal MonthlyTotal => this.Sum(a => a.MonthlyTotal);

    public void SetContractCharge(decimal? contractCharge)
    {
        if (contractCharge.HasValue)
        {
            IsContractCharge = true;
            ContractCharge = contractCharge.Value;
        }
        else
        {
            IsContractCharge = false;
            ContractCharge = 0;
        }
    }
}

public class BillingSummaryContainer
{
    public Container Container { get; set; }
    public ContainerRate Rate { get; set; }
    public decimal MonthlyTotal => Rate.RateAmount + Container.AdditionalCharge;
}

public class BillingSummaryServiceAddress : List<BillingSummaryContainer>
{
    public ServiceAddress ServiceAddress { get; set; }
    public Address Address { get; set; }
    public decimal MonthlyTotal => this.Sum(i => i.MonthlyTotal);
}
