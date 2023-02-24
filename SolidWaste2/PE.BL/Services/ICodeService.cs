using PE.DM;

namespace PE.BL.Services;

public interface ICodeService
{
    Task<ICollection<Code>> GetAll(bool includeDeleted);
    Task<ICollection<Code>> GetByType(string type, bool includeDeleted);
    Task<Code> GetById(int id);
    Task Add(Code code);
    Task Update(Code code);
    Task Remove(Code code);
}
