using Microsoft.EntityFrameworkCore;
using RTools_NTS.Util;
using SW.DAL.Contexts;
using SW.DM;

namespace SW.BLL.Services
{
    public class KanPayService : IKanPayService
    {
        private readonly IDbContextFactory<SwDbContext> contextFactory;

        public KanPayService(IDbContextFactory<SwDbContext> contextFactory)
        {
            this.contextFactory = contextFactory;
        }

        public async Task Add(DM.KanPay kanPay, string userName)
        {
            _ = kanPay ?? throw new ArgumentNullException(nameof(kanPay));
            _ = userName ?? throw new ArgumentNullException(nameof(userName));

            kanPay.KanPayAddDateTime = DateTime.Now;
            kanPay.KanPayAddToi = userName;

            using var db = contextFactory.CreateDbContext();
            db.KanPays.Add(kanPay);
            await db.SaveChangesAsync();
        }

        public async Task<ICollection<DM.KanPay>> GetByToken(string token)
        {
            using var db = contextFactory.CreateDbContext();
            return await db.KanPays
                .Where(e => e.KanPayTokenId == token)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<bool> AnyByCustomer(string customerId)
        {
            using var db = contextFactory.CreateDbContext();
            return await db.KanPays.AnyAsync(e => e.KanPayCid == customerId);
        }

        public async Task DeleteByToken(string token)
        {
            using var db = contextFactory.CreateDbContext();
            var temp = await db.KanPays
                .Where(e => e.KanPayTokenId == token)
                .ToListAsync();

            db.KanPays.RemoveRange(temp);
            await db.SaveChangesAsync();
        }
    }
}
