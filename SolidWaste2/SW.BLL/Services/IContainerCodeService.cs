using SW.DM;

namespace SW.BLL.Services
{
    public interface IContainerCodeService
    {
        Task<ContainerCode> GetById(int containerCodeId);
        Task<ICollection<ContainerCode>> GetAll();
        Task Add(ContainerCode containerCode);
        Task Update(ContainerCode containerCode);
        Task Delete(ContainerCode containerCode);
    }
}
