using SW.DM;

namespace SW.BLL.Services
{
    public interface IPaymentPlanService
    {
        Task<PaymentPlan> GetActiveByCustomer(int customerId, bool includeDetails = true);
        Task<ICollection<PaymentPlan>> GetByCustomer(int customerId, bool includeDetails = true);
    }
}
