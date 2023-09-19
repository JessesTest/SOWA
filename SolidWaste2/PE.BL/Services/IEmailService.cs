using PE.DM;
using System.Threading.Tasks;

namespace PE.BL.Services;

public interface IEmailService
{
    Task<ICollection<Email>> GetAll(bool includeDeleted);
    Task<ICollection<Email>> GetByPerson(int personId, bool includeDeleted);
    Task<Email> GetById(int id);
    Task Add(Email email);
    Task Update(Email email);
    Task Remove(Email email);
    Task SetDefault(int personId, int emailId);
}
