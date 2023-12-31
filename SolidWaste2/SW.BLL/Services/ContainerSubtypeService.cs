﻿using Microsoft.EntityFrameworkCore;
using SW.DAL.Contexts;
using SW.DM;

namespace SW.BLL.Services
{
    public class ContainerSubtypeService : IContainerSubtypeService
    {
        private readonly IDbContextFactory<SwDbContext> dbFactory;

        public ContainerSubtypeService(IDbContextFactory<SwDbContext> dbFactory)
        {
            this.dbFactory = dbFactory;
        }

        public async Task<ICollection<ContainerSubtype>> GetAll()
        {
            using var db = dbFactory.CreateDbContext();
            return await db.ContainerSubtypes
                .Where(e => !e.DeleteFlag)
                .ToListAsync();
        }

        public async Task<ICollection<ContainerSubtype>> GetByContainerType(int containerCodeId)
        {
            using var db = dbFactory.CreateDbContext();
            return await db.ContainerSubtypes
                .Where(e => e.ContainerCodeId == containerCodeId && !e.DeleteFlag)
                .ToListAsync();
        }

        public async Task<ContainerSubtype> GetById(int containerSubtypeId)
        {
            using var db = dbFactory.CreateDbContext();
            return await db.ContainerSubtypes
                .SingleOrDefaultAsync(s => s.ContainerSubtypeId == containerSubtypeId);
        }
    }
}
