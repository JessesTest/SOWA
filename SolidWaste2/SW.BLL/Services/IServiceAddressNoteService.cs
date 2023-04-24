using SW.DM;

namespace SW.BLL.Services
{
    public interface IServiceAddressNoteService
    {
        Task Add(ServiceAddressNote note);
        Task<ServiceAddressNote> GetById(int id);
        Task<ICollection<ServiceAddressNote>> GetByServiceAddress(int serviceAddressId);
        Task Update(ServiceAddressNote note);
    }
}