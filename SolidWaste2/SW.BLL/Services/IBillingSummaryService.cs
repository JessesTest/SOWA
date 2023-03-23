namespace SW.BLL.Services;

public interface IBillingSummaryService
{
    Task<decimal> GetTotalAdjustmentsForCustomerInDateRange(int customerId, DateTime startDate, DateTime endDate);
    Task<decimal> GetTotalNewChargesForCustomerInDateRange(int customerId, DateTime startDate, DateTime endDate);
    Task<decimal> GetTotalPaymentsForCustomerInDateRange(int customerId, DateTime startDate, DateTime endDate);
}
