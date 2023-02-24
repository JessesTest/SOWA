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
    public class PersonEntityService : IPersonEntityService
    {
        private readonly IDbContextFactory<PeDbContext> contextFactory;

        public PersonEntityService(IDbContextFactory<PeDbContext> dbFactory)
        {
            this.contextFactory = dbFactory;
        }

        public Task Add(PersonEntity personEntity)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<PersonEntity>> GetAll(bool includeDeleted)
        {
            throw new NotImplementedException();
        }

        public Task<PersonEntity> GetById(int id)
        {
            throw new NotImplementedException();
        }

        public Task Remove(PersonEntity personEntity)
        {
            throw new NotImplementedException();
        }

        public Task Update(PersonEntity personEntity)
        {
            throw new NotImplementedException();
        }
    }
}
