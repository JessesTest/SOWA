using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SW.DM;

namespace SW.DAL.EntityConfigurations
{
    internal class WorkOrderLegacyEntityConfiguration : IEntityTypeConfiguration<WorkOrderLegacy>
    {
        public void Configure(EntityTypeBuilder<WorkOrderLegacy> entity)
        {
            entity.Property(e => e.AddToi).HasMaxLength(64);

            entity.Property(e => e.BreakdownCode).HasMaxLength(2);

            entity.Property(e => e.BreakdownLocation).HasMaxLength(20);

            entity.Property(e => e.ChgToi).HasMaxLength(64);

            entity.Property(e => e.DelToi).HasMaxLength(64);

            entity.Property(e => e.Driver).HasMaxLength(4);

            entity.Property(e => e.EndTime).HasMaxLength(5);

            entity.Property(e => e.ProblemDescription).HasMaxLength(225);

            entity.Property(e => e.RecType).HasMaxLength(1);

            entity.Property(e => e.ResolveDate).HasColumnType("date");

            entity.Property(e => e.Route).HasMaxLength(5);

            entity.Property(e => e.StartTime).HasMaxLength(5);

            entity.Property(e => e.TransDate).HasColumnType("date");

            entity.Property(e => e.WorkOrderLegacyId).ValueGeneratedOnAdd();
            entity.HasKey(e => e.WorkOrderLegacyId);
        }
    }
}
