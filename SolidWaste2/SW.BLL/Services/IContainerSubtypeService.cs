using SW.DM;

namespace SW.BLL.Services
{
    public interface IContainerSubtypeService
    {
        Task<ICollection<ContainerSubtype>> GetAll();
        Task<ICollection<ContainerSubtype>> GetByContainerType(int containerCodeId);
        Task<ContainerSubtype> GetById(int containerSubtypeId);
    }
}