using SW.DM;

namespace SW.BLL.Services
{
    public interface ITransactionCodeService
    {
        Task<ICollection<TransactionCode>> CollectionPaymentCodes();
        Task<List<TransactionCode>> GetTransactionCodes();
        Task<TransactionCode> GetByCode(string code);
    }
}