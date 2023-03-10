using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SW.DM;

namespace SW.DAL.EntityConfigurations
{
    internal class RouteTypeEntityConfiguration : IEntityTypeConfiguration<RouteType>
    {
        public void Configure(EntityTypeBuilder<RouteType> entity)
        {
            entity.Property(e => e.RouteTypeId).HasColumnName("RouteTypeID");

            entity.Property(e => e.AddDateTime).HasColumnType("datetime");

            entity.Property(e => e.AddToi).HasMaxLength(255);

            entity.Property(e => e.ChgDateTime).HasColumnType("datetime");

            entity.Property(e => e.ChgToi).HasMaxLength(255);

            entity.Property(e => e.DelDateTime).HasColumnType("datetime");

            entity.Property(e => e.DelToi).HasMaxLength(255);

            entity.Property(e => e.Type).HasMaxLength(1);

            entity.Property(e => e.RouteTypeId).ValueGeneratedOnAdd();
            entity.HasKey(e => e.RouteTypeId);
        }
    }
}
