using SW.DM;

namespace SW.BLL.Services
{
    public interface IBillBlobService
    {
        Task<BillBlobs> GetByTransactionId(int transactionId);
    }
}