using SW.DM;

namespace SW.BLL.Services
{
    public interface IPaymentPlanService
    {
        Task<PaymentPlan> GetActiveByCustomer(int customerId, bool includeDetails = true);
        Task<ICollection<PaymentPlan>> GetByCustomer(int customerId, bool includeDetails = true);
        Task<PaymentPlan> GetById(int paymentPlanId);
        Task Add(PaymentPlan pp);
        Task Update(PaymentPlan pp);
    }
}
