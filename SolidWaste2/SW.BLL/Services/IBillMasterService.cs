using SW.DM;

namespace SW.BLL.Services
{
    public interface IBillMasterService
    {
        Task<BillMaster> GetMostRecentBillMaster(int customerId);
        Task<BillMaster> GetPreviousBillMaster(int customerId);
    }
}