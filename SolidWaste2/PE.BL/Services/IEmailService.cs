using PE.DM;

namespace PE.BL.Services;

public interface IEmailService
{
    Task<ICollection<Email>> GetAll(bool includeDeleted);
    Task<ICollection<Email>> GetByPerson(int personId, bool includeDeleted);
    Task<Email> GetById(int id);
    void Add(Email email);
    void Update(Email email);
    void Remove(Email email);
}
