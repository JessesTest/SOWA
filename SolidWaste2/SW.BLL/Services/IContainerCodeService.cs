using SW.DM;

namespace SW.BLL.Services
{
    public interface IContainerCodeService
    {
        Task<ContainerCode> GetById(int containerCodeId);
    }
}