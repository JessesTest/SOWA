using SW.DM;

namespace SW.BLL.Services;

public interface IBillContainerService
{
    Task<ICollection<BillContainerDetail>> GetByBillServiceAddress(int billServiceAddressId);
}