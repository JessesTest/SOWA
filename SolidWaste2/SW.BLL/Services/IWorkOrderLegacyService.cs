using SW.BLL.DTOs;
using SW.DM;

namespace SW.BLL.Services;

public interface IWorkOrderLegacyService
{
    Task<WorkOrderLegacy> GetById(int workOrderLegacyId);
    Task Update(WorkOrderLegacy workOrderLegacy);
    Task<ICollection<WorkOrderLegacy>> GetInquiryResultList(int? equipmentNumber, string route, DateTime? transDate, string driver, string breakdownLocation, int? problemNumber, bool include);
}
