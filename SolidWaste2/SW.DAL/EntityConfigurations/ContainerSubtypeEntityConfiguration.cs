using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SW.DM;

namespace SW.DAL.EntityConfigurations
{
    internal class ContainerSubtypeEntityConfiguration : IEntityTypeConfiguration<ContainerSubtype>
    {
        public void Configure(EntityTypeBuilder<ContainerSubtype> entity)
        {
            entity.HasIndex(e => e.ContainerCodeId, "IX_ContainerCodeId");

            entity.Property(e => e.ContainerSubtypeId).HasColumnName("ContainerSubtypeID");

            entity.Property(e => e.AddDateTime).HasColumnType("datetime");

            entity.Property(e => e.AddToi).HasMaxLength(255);

            entity.Property(e => e.BillingFrequency).HasMaxLength(10);

            entity.Property(e => e.ChgDateTime).HasColumnType("datetime");

            entity.Property(e => e.ChgToi).HasMaxLength(255);

            entity.Property(e => e.DelDateTime).HasColumnType("datetime");

            entity.Property(e => e.DelToi).HasMaxLength(255);

            entity.Property(e => e.Description).HasMaxLength(40);

            entity.HasOne(d => d.ContainerCode)
                .WithMany(p => p.ContainerSubtypes)
                .HasForeignKey(d => d.ContainerCodeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_dbo.ContainerSubtypes_dbo.ContainerCodes_ContainerCodeId");

            entity.Property(e => e.ContainerSubtypeId).ValueGeneratedOnAdd();
            entity.HasKey(e => e.ContainerSubtypeId);
        }
    }
}
