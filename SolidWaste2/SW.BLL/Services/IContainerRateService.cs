using SW.DM;

namespace SW.BLL.Services;

public interface IContainerRateService
{
    Task Add(ContainerRate containerRate);
    Task Delete(ContainerRate containerRate);
    Task<ICollection<ContainerRate>> GetAll();
    Task<ContainerRate> GetById(int id);
    Task<ICollection<ContainerRate>> GetContainerRateByCodeDaysSize(int containerSubtypeId, int dayCount, decimal billingSize, DateTime effective_date);
    Task<ICollection<ContainerRate>> GetContainerRateByCodeDays(int containerCodeId, int dayCount);
    Task<ICollection<int>> GetDaysOfServiceByContainerSubtype(int containerSubtypeId);
    Task<ICollection<ContainerRateListing>> GetListing();
    Task Update(ContainerRate containerRate);
}

public class ContainerRateListing
{
    public int ContainerRateId { get; set; }

    public string TypeCode { get; set; } 
    public string TypeDescription { get; set; }
    public string SubtypeFrequency { get; set; }
    public string SubtypeDescription { get; set; }

    public decimal BillingSize { get; set; } 
    public int? NumDaysService { get; set; }
    public string Description { get; set; }
    public decimal Amount { get; set; }
    public decimal PullCharge { get; set; }
    public DateTime EffectiveDate { get; set; }
}
