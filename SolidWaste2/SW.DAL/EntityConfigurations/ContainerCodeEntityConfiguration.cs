using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SW.DM;

namespace SW.DAL.EntityConfigurations
{
    internal class ContainerCodeEntityConfiguration : IEntityTypeConfiguration<ContainerCode>
    {
        public void Configure(EntityTypeBuilder<ContainerCode> entity)
        {
            entity.Property(e => e.ContainerCodeId).HasColumnName("ContainerCodeID");

            entity.Property(e => e.AddDateTime).HasColumnType("datetime");

            entity.Property(e => e.AddToi).HasMaxLength(255);

            entity.Property(e => e.BillingFrequency).HasMaxLength(8);

            entity.Property(e => e.ChgDateTime).HasColumnType("datetime");

            entity.Property(e => e.ChgToi).HasMaxLength(255);

            entity.Property(e => e.DelDateTime).HasColumnType("datetime");

            entity.Property(e => e.DelToi).HasMaxLength(255);

            entity.Property(e => e.Description).HasMaxLength(50);

            entity.Property(e => e.Type).HasMaxLength(1);

            entity.Property(e => e. ContainerCodeId).ValueGeneratedOnAdd();
            entity.HasKey(e => e.ContainerCodeId);
        }
    }
}
