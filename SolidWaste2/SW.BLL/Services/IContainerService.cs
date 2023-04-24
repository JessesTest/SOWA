using PE.DM;
using SW.DM;

namespace SW.BLL.Services
{
    public interface IContainerService
    {
        Task Add(Container c);
        Task CustomerCancelContainer(DateTime cancelDate, PersonEntity person, int containerId, string userName);
        Task<ICollection<Container>> GetByCustomer(int customerId);
        Task<Container> GetById(int containerId);
        Task<Container> GetById(int containerId, int customerId);
        Task<ICollection<Container>> GetByServiceAddress(int serviceAddressId);
        Task<string> TryValidateContainer(Container c);
        Task Update(Container c);
    }
}
