using PE.DM;

namespace PE.BL.Services;

public interface IPersonEntityService
{
    Task<IEnumerable<PersonEntity>> GetAll(bool includeDeleted);
    Task<PersonEntity> GetById(int id);
    Task Add(PersonEntity personEntity);
    Task Update(PersonEntity personEntity);
    Task Remove(PersonEntity personEntity);
}
