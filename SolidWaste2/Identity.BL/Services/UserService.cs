using Identity.DAL.Contexts;
using Identity.DM;
using Microsoft.EntityFrameworkCore;
using PE.BL.Services;

namespace Identity.BL.Services
{
    public class UserService : IUserService
    {
        private readonly IDbContextFactory<IdentityDbContext> contextFactory;
        private readonly IPersonEntityService personEntityService;
        private readonly IEmailService emailService;
        private readonly ICodeService codeService;

        public UserService(
            IDbContextFactory<IdentityDbContext> contextFactory,
            IPersonEntityService personEntityService,
            IEmailService emailService,
            ICodeService codeService)
        {
            this.contextFactory = contextFactory;
            this.personEntityService = personEntityService;
            this.emailService = emailService;
            this.codeService = codeService;
        }

        public async Task<AspNetUser> GeByUserName(string userName)
        {
            using var db = contextFactory.CreateDbContext();
            return await db.AspNetUsers
                .Where(e => e.UserName == userName)
                .AsNoTracking()
                .SingleOrDefaultAsync();
        }

        public async Task<AspNetUser> GetByUserId(int userId)
        {
            using var db = contextFactory.CreateDbContext();
            return await db.AspNetUsers
                .Where(m => m.UserId == userId)
                .AsNoTracking()
                .SingleOrDefaultAsync();
        }

        public async Task<bool> UserNameExists(string userName)
        {
            using var db = contextFactory.CreateDbContext();
            return await db.AspNetUsers
                .Where(e => e.UserName == userName)
                .AnyAsync();
        }

        public async Task Update(AspNetUser user)
        {
            using var db = contextFactory.CreateDbContext();
            db.AspNetUsers.Update(user);
            await db.SaveChangesAsync();
        }

        public Task DeleteByUserId(int userId)
        {
            throw new NotImplementedException();
        }

        public Task DeleteByUserName(string userName)
        {
            throw new NotImplementedException();
        }

        public Task Delete(AspNetUser user)
        {
            throw new NotImplementedException();
        }

        public Task<AspNetUser> GetByEmail(string email)
        {
            throw new NotImplementedException();
        }

        public async Task<bool> ConfirmEmail(int userId, string code)
        {
            if (userId == 0 || code == null)
                return false;

            var user = await GetByUserId(userId);
            if (user.EmailConfirmed)
            {
                user.EmailConfirmationCode = null;
                await Update(user);
                return true;
            }

            if (user.EmailConfirmationCode == null)
                return false;

            if (user.EmailConfirmationCode != code)
                return false;

            // Add the email address to PE if it is not already there
            var emails = await emailService.GetByPerson(user.UserId, false);
            var hasEmail = emails.Any(e => e.Email1.Equals(user.Email, StringComparison.InvariantCultureIgnoreCase));

            if (!hasEmail)
            {
                var email = new PE.DM.Email
                {
                    Email1 = user.Email?.ToLower(),
                    AddToi = "Confirmation",
                    Type = (await codeService.GetByType("Email", false)).Single(e => e.Description == "Personal").Id,
                    PersonEntityID = user.UserId,
                    Status = true
                };
                await emailService.Add(email);
                await personEntityService.SetDefaultEmail(user.UserId, email.Id);
            }

            user.EmailConfirmationCode = null;
            user.EmailConfirmed = true;
            await Update(user);

            return true;
        }

        public async Task<ICollection<AspNetUser>> FindAllByEmail(string email)
        {
            using var db = contextFactory.CreateDbContext();
            return await db.AspNetUsers
                .Where(e => e.Email == email)
                .AsNoTracking()
                .ToListAsync();
        }
    }
}
