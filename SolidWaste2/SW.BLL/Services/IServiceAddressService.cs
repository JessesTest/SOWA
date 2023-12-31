﻿using PE.DM;
using SW.DM;

namespace SW.BLL.Services;

public interface IServiceAddressService
{
    Task CustomerCancelServiceAddress(DateTime cancelDate, PersonEntity person, int serviceAddressId, string userName);
    Task<ICollection<ServiceAddress>> GetByCustomer(int customerId);
    Task<ServiceAddress> GetById(int serviceAddressId);
    Task Update(ServiceAddress serviceAddress);
    Task<string> TryValidateServiceAddress(ServiceAddress sa);
    Task Add(ServiceAddress serviceAddress);
}
