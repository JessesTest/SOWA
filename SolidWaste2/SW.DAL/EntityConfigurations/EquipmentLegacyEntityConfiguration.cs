using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SW.DM;

namespace SW.DAL.EntityConfigurations
{
    internal class EquipmentLegacyEntityConfiguration : IEntityTypeConfiguration<EquipmentLegacy>
    {
        public void Configure(EntityTypeBuilder<EquipmentLegacy> entity)
        {
            entity.Property(e => e.AddToi).HasMaxLength(64);

            entity.Property(e => e.ChgToi).HasMaxLength(64);

            entity.Property(e => e.DateOfLastPm)
                .HasColumnType("date")
                .HasColumnName("DateOfLastPM");

            entity.Property(e => e.DelToi).HasMaxLength(64);

            entity.Property(e => e.EquipmentDescription).HasMaxLength(20);

            entity.Property(e => e.EquipmentMake).HasMaxLength(20);

            entity.Property(e => e.EquipmentModel).HasMaxLength(20);

            entity.Property(e => e.EquipmentVin)
                .HasMaxLength(20)
                .HasColumnName("EquipmentVIN");

            entity.Property(e => e.EquipmentYear).HasMaxLength(2);

            entity.Property(e => e.MilesAtLastPm).HasColumnName("MilesAtLastPM");

            entity.Property(e => e.MonthlyDepreciationAmt).HasColumnType("decimal(18, 2)");

            entity.Property(e => e.PurchaseAmt).HasColumnType("decimal(18, 2)");

            entity.Property(e => e.PurchaseDate).HasColumnType("date");

            entity.Property(e => e.StandbyEquipmentFlag).HasMaxLength(2);

            entity.Property(e => e.EquipmentLegacyId).ValueGeneratedOnAdd();
            entity.HasKey(e => e.EquipmentLegacyId);
        }
    }
}
