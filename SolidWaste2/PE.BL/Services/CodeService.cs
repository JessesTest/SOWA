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
    public class CodeService : ICodeService
    {
        private readonly IDbContextFactory<PeDbContext> contextFactory;

        public CodeService(IDbContextFactory<PeDbContext> dbFactory)
        {
            this.contextFactory = dbFactory;
        }

        public Task Add(Code code)
        {
            throw new NotImplementedException();
        }

        public Task<ICollection<Code>> GetAll(bool includeDeleted)
        {
            throw new NotImplementedException();
        }

        public Task<Code> GetById(int id)
        {
            throw new NotImplementedException();
        }

        public Task<ICollection<Code>> GetByType(string type, bool includeDeleted)
        {
            throw new NotImplementedException();
        }

        public Task Remove(Code code)
        {
            throw new NotImplementedException();
        }

        public Task Update(Code code)
        {
            throw new NotImplementedException();
        }
    }
}
