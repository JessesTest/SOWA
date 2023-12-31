﻿using PE.DM;

namespace PE.BL.Services;

public interface IAddressService
{
    Task<ICollection<Address>> GetAll(bool includeDeleted);
    Task<ICollection<Address>> GetByPerson(int personId, bool includeDeleted);
    Task<Address> GetById(int id);
    Task Add(Address address);
    Task Update(Address address);
    Task Remove(Address address);
    Task SetDefault(int personId, int addressId);
    Task<ICollection<Address>> GetByIds(IEnumerable<int> ids);
}
