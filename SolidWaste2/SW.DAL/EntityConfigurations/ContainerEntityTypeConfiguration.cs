using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SW.DM;

namespace SW.DAL.EntityConfigurations
{
    internal class ContainerEntityTypeConfiguration : IEntityTypeConfiguration<Container>
    {
        public void Configure(EntityTypeBuilder<Container> entity)
        {
            entity.HasIndex(e => e.ContainerCodeId, "IX_ContainerCodeId");

            entity.HasIndex(e => e.ContainerSubtypeId, "IX_ContainerSubtypeID");

            entity.HasIndex(e => e.ServiceAddressId, "IX_ServiceAddressId");

            entity.Property(e => e.ActualSize).HasColumnType("decimal(3, 1)");

            entity.Property(e => e.AddDateTime).HasColumnType("datetime");

            entity.Property(e => e.AddToi).HasMaxLength(255);

            entity.Property(e => e.AdditionalCharge).HasColumnType("decimal(18, 2)");

            entity.Property(e => e.BillingSize).HasColumnType("decimal(3, 1)");

            entity.Property(e => e.CancelDate).HasColumnType("datetime");

            entity.Property(e => e.ChgDateTime).HasColumnType("datetime");

            entity.Property(e => e.ChgToi).HasMaxLength(255);

            entity.Property(e => e.ContainerSubtypeId).HasColumnName("ContainerSubtypeID");

            entity.Property(e => e.DelDateTime).HasColumnType("datetime");

            entity.Property(e => e.DelToi).HasMaxLength(255);

            entity.Property(e => e.Delivered).HasMaxLength(255);

            entity.Property(e => e.EffectiveDate).HasColumnType("datetime");

            entity.Property(e => e.RouteNumber).HasMaxLength(50);

            entity.HasOne(d => d.ContainerCode)
                .WithMany(p => p.Containers)
                .HasForeignKey(d => d.ContainerCodeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_dbo.Containers_dbo.ContainerCodes_ContainerCodeId");

            entity.HasOne(d => d.ContainerSubtype)
                .WithMany(p => p.Containers)
                .HasForeignKey(d => d.ContainerSubtypeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_dbo.Containers_dbo.ContainerSubtypes_ContainerSubtypeID");

            entity.HasOne(d => d.ServiceAddress)
                .WithMany(p => p.Containers)
                .HasForeignKey(d => d.ServiceAddressId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_dbo.Containers_dbo.ServiceAddresses_ServiceAddressId");

            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            entity.HasKey(e => e.Id);
        }
    }
}
