using Microsoft.EntityFrameworkCore;
using PE.DM;

namespace PE.DAL.Contexts;

public class PeDbContext : DbContext
{
    public virtual DbSet<Address> Addresses { get; set; }
    public virtual DbSet<Code> Codes { get; set; }
    public virtual DbSet<Email> Emails { get; set; }
    public virtual DbSet<Phone> Phones { get; set; }
    public virtual DbSet<PersonEntity> People { get; set; }

    public PeDbContext(DbContextOptions<PeDbContext> options) : base(options) { }
}
