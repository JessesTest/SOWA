using SW.DM;

namespace SW.BLL.Services;

public interface ITransactionCodeService
{
    Task<ICollection<TransactionCode>> CollectionPaymentCodes();
    Task<ICollection<TransactionCode>> GetAll();
    Task<TransactionCode> GetById(int id);
    Task<TransactionCode> GetByCode(string code);
    Task Add(TransactionCode transactionCode);
    Task Update(TransactionCode transactionCode);
    Task Delete(TransactionCode transactionCode);
}