using SW.DM;

namespace SW.BLL.Services;

public interface IWorkOrderService
{
    Task<WorkOrder> GetById(int id);
    Task Add(WorkOrder workOrder);
    Task Update(WorkOrder workOrder);
    Task Delete(WorkOrder workOrder);
    Task<ICollection<WorkOrder>> GetInquiryResultList(int? workOrderId, string containerRoute, DateTime? transDate, string driverInitials, string customerName, string customerAddress, bool include);
}
