using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SW.DM;

namespace SW.DAL.EntityConfigurations
{
    internal class CustomerEntityConfiguration : IEntityTypeConfiguration<Customer>
    {
        public void Configure(EntityTypeBuilder<Customer> entity)
        {
            entity.HasKey(e => new { e.CustomerType, e.CustomerId })
                .HasName("PK_dbo.Customers");

            entity.Property(e => e.CustomerType).HasMaxLength(128);

            entity.Property(e => e.CustomerId).HasColumnName("CustomerID");

            entity.Property(e => e.AddDateTime).HasColumnType("datetime");

            entity.Property(e => e.AddToi).HasMaxLength(255);

            entity.Property(e => e.CancelDate).HasColumnType("datetime");

            entity.Property(e => e.ChgDateTime).HasColumnType("datetime");

            entity.Property(e => e.ChgToi).HasMaxLength(255);

            entity.Property(e => e.Contact).HasMaxLength(255);

            entity.Property(e => e.ContractCharge).HasColumnType("decimal(18, 2)");

            entity.Property(e => e.DelDateTime).HasColumnType("datetime");

            entity.Property(e => e.DelToi).HasMaxLength(255);

            entity.Property(e => e.EffectiveDate).HasColumnType("datetime");

            entity.Property(e => e.LegacyCustomerId).HasColumnName("LegacyCustomerID");

            entity.Property(e => e.NameAttn).HasMaxLength(255);

            entity.Property(e => e.Notes).HasMaxLength(255);

            entity.Property(e => e.Pe).HasColumnName("PE");

            entity.Property(e => e.PurchaseOrder).HasMaxLength(16);
        }
    }
}
