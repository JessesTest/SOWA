using SW.DM;

namespace SW.BLL.Services;

public interface IKanPayService
{
    Task<ICollection<DM.KanPay>> GetByToken(string token);
    Task<bool> AnyByCustomer(string customerId);
    Task Add(DM.KanPay kanPay, string userName);
    Task DeleteByToken(string token);
}
