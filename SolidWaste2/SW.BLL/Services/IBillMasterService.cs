using SW.DM;

namespace SW.BLL.Services
{
    public interface IBillMasterService
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="transactionId"></param>
        /// <returns>bill master, bill servcie addresses, and bill container details</returns>
        Task<BillMaster> GetByTransaction(int transactionId);
        Task<BillMaster> GetMostRecentBillMaster(int customerId);
        Task<BillMaster> GetPreviousBillMaster(int customerId);
    }
}