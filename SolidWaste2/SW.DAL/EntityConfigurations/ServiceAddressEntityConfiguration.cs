using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SW.DM;

namespace SW.DAL.EntityConfigurations
{
    internal class ServiceAddressEntityConfiguration : IEntityTypeConfiguration<ServiceAddress>
    {
        public void Configure(EntityTypeBuilder<ServiceAddress> entity)
        {
            entity.HasIndex(e => new { e.CustomerType, e.CustomerId }, "IX_CustomerType_CustomerId");

            entity.HasIndex(e => new { e.CustomerId, e.DeleteFlag }, "KOW5");

            entity.Property(e => e.AddDateTime).HasColumnType("datetime");

            entity.Property(e => e.AddToi).HasMaxLength(255);

            entity.Property(e => e.CancelDate).HasColumnType("datetime");

            entity.Property(e => e.ChgDateTime).HasColumnType("datetime");

            entity.Property(e => e.ChgToi).HasMaxLength(255);

            entity.Property(e => e.CustomerType).HasMaxLength(128);

            entity.Property(e => e.DelDateTime).HasColumnType("datetime");

            entity.Property(e => e.DelToi).HasMaxLength(255);

            entity.Property(e => e.EffectiveDate).HasColumnType("datetime");

            entity.Property(e => e.Email).HasMaxLength(100);

            entity.Property(e => e.LocationContact).HasMaxLength(50);

            entity.Property(e => e.LocationName).HasMaxLength(50);

            entity.Property(e => e.LocationNumber).HasMaxLength(50);

            entity.Property(e => e.PeaddressId).HasColumnName("PEAddressId");

            entity.Property(e => e.Phone).HasMaxLength(20);

            entity.Property(e => e.ServiceType).HasMaxLength(50);

            entity.HasOne(d => d.Customer)
                .WithMany(p => p.ServiceAddresses)
                .HasForeignKey(d => new { d.CustomerType, d.CustomerId })
                .HasConstraintName("FK_dbo.ServiceAddresses_dbo.Customers_CustomerType_CustomerId");

            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            entity.HasKey(e => e.Id);
        }
    }
}
