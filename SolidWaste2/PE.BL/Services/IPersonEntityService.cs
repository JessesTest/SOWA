using PE.DM;

namespace PE.BL.Services;

public interface IPersonEntityService
{
    Task<IEnumerable<PersonEntity>> GetAll(bool includeDeleted);
    Task<PersonEntity> GetById(int id);
    Task Add(PersonEntity personEntity);
    Task Update(PersonEntity personEntity);
    Task Remove(PersonEntity personEntity);
    Task<PersonEntity> GetBySystemAndCode(string system, int code);

    Task SetDefaultPhone(int personEntityId, int phoneId);
    Task SetDefaultEmail(int personEntityId, int emailId);
    Task SetDefaultAddress(int personEntityId, int addressId);

    Task<ICollection<PersonEntity>> Search(
            string fullName,
            string firstName,
            string middleName,
            string lastName,
            string billingAddress,
            string serviceAddress,
            string departmentCodeCode,
            string pin = null,
            IEnumerable<int> personEntityIds = null);
}
