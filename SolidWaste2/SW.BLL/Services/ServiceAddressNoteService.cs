using Common.Extensions;
using Microsoft.EntityFrameworkCore;
using SW.DAL.Contexts;
using SW.DM;

namespace SW.BLL.Services;

public class ServiceAddressNoteService : IServiceAddressNoteService
{
    private readonly IDbContextFactory<SwDbContext> dbFactory;

    public ServiceAddressNoteService(IDbContextFactory<SwDbContext> dbFactory)
    {
        this.dbFactory = dbFactory;
    }

    public async Task<ServiceAddressNote> GetById(int id)
    {
        using var db = dbFactory.CreateDbContext();
        return await db.ServiceAddressNotes.FindAsync(id);
    }

    public async Task<ICollection<ServiceAddressNote>> GetByServiceAddress(int serviceAddressId)
    {
        using var db = dbFactory.CreateDbContext();
        return await db.ServiceAddressNotes
            .Where(n => n.ServiceAddressId == serviceAddressId && !n.DeleteFlag)
            .OrderByDescending(n => n.AddDateTime)
            .ToListAsync();
    }

    public async Task Add(ServiceAddressNote note)
    {
        note.AddToi ??= System.Security.Claims.ClaimsPrincipal.Current.GetNameOrEmail();

        using var db = dbFactory.CreateDbContext();
        db.ServiceAddressNotes.Add(note);
        await db.SaveChangesAsync();
    }

    public async Task Update(ServiceAddressNote note)
    {
        note.ChgDateTime ??= DateTime.Now;
        note.ChgToi ??= System.Security.Claims.ClaimsPrincipal.Current.GetNameOrEmail();

        var sa = note.ServiceAddress;
        note.ServiceAddress = null;

        using var db = dbFactory.CreateDbContext();
        db.ServiceAddressNotes.Update(note);
        await db.SaveChangesAsync();

        note.ServiceAddress = sa;
    }
}
