using SW.DM;

namespace SW.BLL.Services;

public interface IWorkOrderService
{
    Task<ICollection<WorkOrder>> GetInquiryResultList(int? workOrderId, string containerRoute, DateTime? transDate, string driverInitials, string customerName, string customerAddress, bool include);
}
