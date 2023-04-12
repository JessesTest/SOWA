using SW.DM;

namespace SW.BLL.Services;

public interface IWorkOrderLegacyService
{
    Task<WorkOrderLegacy> GetById(int workOrderLegacyId);
    Task Update(WorkOrderLegacy workOrderLegacy);
}
