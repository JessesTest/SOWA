using SW.DM;

namespace SW.BLL.Services;

public interface IEquipmentLegacyService
{
    Task<ICollection<EquipmentLegacy>> GetAll();
    Task<EquipmentLegacy> GetById(int id);
    Task<EquipmentLegacy> GetByEquipmentNumber(int equipmentNumber);
}
