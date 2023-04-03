using SW.DM;

namespace SW.BLL.Services
{
    public interface IContainerSubtypeService
    {
        Task<ContainerSubtype> GetById(int containerSubtypeId);
    }
}