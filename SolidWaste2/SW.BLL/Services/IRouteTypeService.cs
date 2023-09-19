using SW.DM;

namespace SW.BLL.Services;

public interface IRouteTypeService
{
    Task<ICollection<RouteType>> GetAll();
    Task<RouteType> GetById(int id);
    Task<RouteType> GetByRouteNumber(int routeNumber);
    Task Update(RouteType type);
    Task Add(RouteType type);
}
