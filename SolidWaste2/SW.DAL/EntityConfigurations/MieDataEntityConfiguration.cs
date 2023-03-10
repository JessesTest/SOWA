using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SW.DM;

namespace SW.DAL.EntityConfigurations
{
    internal class MieDataEntityConfiguration : IEntityTypeConfiguration<MieData>
    {
        public void Configure(EntityTypeBuilder<MieData> entity)
        {
            entity.Property(e => e.MieDataId).HasColumnName("MieDataID");

            entity.Property(e => e.AddDateTime).HasColumnType("datetime");

            entity.Property(e => e.AddToi).HasMaxLength(255);

            entity.Property(e => e.ChgDateTime).HasColumnType("datetime");

            entity.Property(e => e.ChgToi).HasMaxLength(255);

            entity.Property(e => e.DelDateTime).HasColumnType("datetime");

            entity.Property(e => e.DelToi).HasMaxLength(255);

            entity.Property(e => e.MieDataImageFileName).HasMaxLength(255);

            entity.Property(e => e.MieDataImageId)
                .HasMaxLength(255)
                .HasColumnName("MieDataImageID");

            entity.Property(e => e.MieDataImageType).HasMaxLength(255);

            entity.Property(e => e.MieDataId).ValueGeneratedOnAdd();
            entity.HasKey(e => e.MieDataId);
        }
    }
}
