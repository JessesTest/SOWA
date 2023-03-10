using PE.DM;

namespace PE.BL.Services;

public interface IPhoneService
{
    Task<ICollection<Phone>> GetAll(bool includeDeleted);
    Task<ICollection<Phone>> GetByPerson(int personId, bool includeDeleted);
    Task<Phone> GetById(int id);
    Task Add(Phone phone);
    Task Update(Phone phone);
    Task Remove(Phone phone);
}
