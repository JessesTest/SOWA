using Microsoft.EntityFrameworkCore;
using Notify.DM;

namespace Notify.DAL.Contexts;

public class NotifyDbContext : DbContext
{
    public virtual DbSet<Notification> Notifications { get; set; }

    public NotifyDbContext(DbContextOptions<NotifyDbContext> options) : base(options) { }
}
