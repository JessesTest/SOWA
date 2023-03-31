using SW.DM;

namespace SW.BLL.Services
{
    public interface IContainerRateService
    {
        Task<ICollection<ContainerRate>> GetContainerRateByCodeDaysSize(int containerSubtypeId, int dayCount, decimal billingSize, DateTime effective_date);
        Task<ICollection<int>> GetDaysOfServiceByContainerSubtype(int containerSubtypeId);
    }
}