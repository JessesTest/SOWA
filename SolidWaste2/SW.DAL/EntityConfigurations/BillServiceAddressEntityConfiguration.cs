using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SW.DM;

namespace SW.DAL.EntityConfigurations
{
    internal class BillServiceAddressEntityConfiguration : IEntityTypeConfiguration<BillServiceAddress>
    {
        public void Configure(EntityTypeBuilder<BillServiceAddress> entity)
        {
            entity.HasIndex(e => e.BillMasterId, "IX_BillMasterId");

            entity.HasIndex(e => e.ServiceAddressId, "IX_ServiceAddressID");

            entity.Property(e => e.AddDateTime).HasColumnType("datetime");

            entity.Property(e => e.AddToi).HasMaxLength(255);

            entity.Property(e => e.ChgDateTime).HasColumnType("datetime");

            entity.Property(e => e.ChgToi).HasMaxLength(255);

            entity.Property(e => e.DelDateTime).HasColumnType("datetime");

            entity.Property(e => e.DelToi).HasMaxLength(255);

            entity.Property(e => e.ServiceAddressCityStateZip).HasMaxLength(100);

            entity.Property(e => e.ServiceAddressId).HasColumnName("ServiceAddressID");

            entity.Property(e => e.ServiceAddressName).HasMaxLength(100);

            entity.Property(e => e.ServiceAddressStreet).HasMaxLength(100);

            entity.HasOne(d => d.BillMaster)
                .WithMany(p => p.BillServiceAddresses)
                .HasForeignKey(d => d.BillMasterId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_dbo.BillServiceAddresses_dbo.BillMasters_BillMasterId");

            entity.HasOne(d => d.ServiceAddress)
                .WithMany(p => p.BillServiceAddresses)
                .HasForeignKey(d => d.ServiceAddressId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_dbo.BillServiceAddresses_dbo.ServiceAddresses_ServiceAddressID");

            entity.Property(e => e.BillServiceAddressId).ValueGeneratedOnAdd();
            entity.HasKey(e => e.BillServiceAddressId);
        }
    }
}
