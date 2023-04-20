using SW.DM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SW.BLL.Services;

public interface IRouteTypeService
{
    Task<ICollection<RouteType>> GetAll();
}
