using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SW.DM;

namespace SW.DAL.EntityConfigurations
{
    internal class ContainerRateEntityConfiguration : IEntityTypeConfiguration<ContainerRate>
    {
        public void Configure(EntityTypeBuilder<ContainerRate> entity)
        {
            entity.HasIndex(e => e.ContainerSubtypeId, "IX_ContainerSubtypeID");

            entity.HasIndex(e => e.ContainerType, "IX_ContainerType");

            entity.Property(e => e.ContainerRateId).HasColumnName("ContainerRateID");

            entity.Property(e => e.AddDateTime).HasColumnType("datetime");

            entity.Property(e => e.AddToi).HasMaxLength(255);

            entity.Property(e => e.BillingSize).HasColumnType("decimal(3, 1)");

            entity.Property(e => e.ChgDateTime).HasColumnType("datetime");

            entity.Property(e => e.ChgToi).HasMaxLength(255);

            entity.Property(e => e.ContainerSubtypeId).HasColumnName("ContainerSubtypeID");

            entity.Property(e => e.DelDateTime).HasColumnType("datetime");

            entity.Property(e => e.DelToi).HasMaxLength(255);

            entity.Property(e => e.EffectiveDate).HasColumnType("date");

            entity.Property(e => e.ExtraPickup).HasColumnType("decimal(18, 2)");

            entity.Property(e => e.PullCharge).HasColumnType("decimal(18, 2)");

            entity.Property(e => e.RateAmount).HasColumnType("decimal(18, 2)");

            entity.Property(e => e.RateDescription).HasMaxLength(50);

            entity.HasOne(d => d.ContainerSubtype)
                .WithMany(p => p.ContainerRates)
                .HasForeignKey(d => d.ContainerSubtypeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_dbo.ContainerRates_dbo.ContainerSubtypes_ContainerSubtypeID");

            entity.HasOne(d => d.ContainerTypeNavigation)
                .WithMany(p => p.ContainerRates)
                .HasForeignKey(d => d.ContainerType)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_dbo.ContainerRates_dbo.ContainerCodes_ContainerType");

            entity.Property(e => e.ContainerRateId).ValueGeneratedOnAdd();
            entity.HasKey(e => e.ContainerRateId);
        }
    }
}
