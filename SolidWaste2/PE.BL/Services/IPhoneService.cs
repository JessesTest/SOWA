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

    Task SetDefaultPhone(int personEntityId, int phoneId);
    Task SetDefaultEmail(int personEntityId, int emailId);
    Task SetDefaultAddress(int personEntityId, int addressId);
}
