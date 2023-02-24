using Microsoft.EntityFrameworkCore;
using PE.DAL.Contexts;
using PE.DM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PE.BL.Services
{
    public class AddressService : IAddressService
    {
        private readonly IDbContextFactory<PeDbContext> contextFactory;

        public AddressService(IDbContextFactory<PeDbContext> dbFactory)
        {
            this.contextFactory = dbFactory;
        }

        public Task Add(Address address)
        {
            throw new NotImplementedException();
        }

        public Task<ICollection<Address>> GetAll(bool includeDeleted)
        {
            throw new NotImplementedException();
        }

        public Task<Address> GetById(int id)
        {
            throw new NotImplementedException();
        }

        public Task<ICollection<Address>> GetByPerson(int personId, bool includeDeleted)
        {
            throw new NotImplementedException();
        }

        public Task Remove(Address address)
        {
            throw new NotImplementedException();
        }

        public Task Update(Address address)
        {
            throw new NotImplementedException();
        }
    }
}
