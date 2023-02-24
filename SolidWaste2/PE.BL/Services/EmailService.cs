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
    public class EmailService : IEmailService
    {
        private readonly IDbContextFactory<PeDbContext> contextFactory;

        public EmailService(IDbContextFactory<PeDbContext> contextFactory)
        {
            this.contextFactory = contextFactory;
        }

        public void Add(Email email)
        {
            throw new NotImplementedException();
        }

        public Task<ICollection<Email>> GetAll(bool includeDeleted)
        {
            throw new NotImplementedException();
        }

        public Task<Email> GetById(int id)
        {
            throw new NotImplementedException();
        }

        public Task<ICollection<Email>> GetByPerson(int personId, bool includeDeleted)
        {
            throw new NotImplementedException();
        }

        public void Remove(Email email)
        {
            throw new NotImplementedException();
        }

        public void Update(Email email)
        {
            throw new NotImplementedException();
        }
    }
}
