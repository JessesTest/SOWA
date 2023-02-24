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
    public class PhoneService : IPhoneService
    {
        private readonly IDbContextFactory<PeDbContext> contextFactory;

        public PhoneService(IDbContextFactory<PeDbContext> contextFactory)
        {
            this.contextFactory = contextFactory;
        }

        public Task Add(Phone phone)
        {
            throw new NotImplementedException();
        }

        public Task<ICollection<Phone>> GetAll(bool includeDeleted)
        {
            throw new NotImplementedException();
        }

        public Task<Phone> GetById(int id)
        {
            throw new NotImplementedException();
        }

        public Task<ICollection<Phone>> GetByPerson(int personId, bool includeDeleted)
        {
            throw new NotImplementedException();
        }

        public Task Remove(Phone phone)
        {
            throw new NotImplementedException();
        }

        public Task SetDefaultAddress(int personEntityId, int addressId)
        {
            throw new NotImplementedException();
        }

        public Task SetDefaultEmail(int personEntityId, int emailId)
        {
            throw new NotImplementedException();
        }

        public Task SetDefaultPhone(int personEntityId, int phoneId)
        {
            throw new NotImplementedException();
        }

        public Task Update(Phone phone)
        {
            throw new NotImplementedException();
        }
    }
}
