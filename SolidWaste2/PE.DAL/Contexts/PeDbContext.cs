using Microsoft.EntityFrameworkCore;
using PE.DM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PE.DAL.Contexts
{
    public class PeDbContext : DbContext
    {
        public virtual DbSet<Address> Addresses { get; set; }
        public virtual DbSet<Code> Codes { get; set; }
        public virtual DbSet<Email> Emails { get; set; }
        public virtual DbSet<Phone> Phones { get; set; }
        public virtual DbSet<PersonEntity> People { get; set; }

        public PeDbContext(DbContextOptions<PeDbContext> options) : base(options) { }
    }
}
