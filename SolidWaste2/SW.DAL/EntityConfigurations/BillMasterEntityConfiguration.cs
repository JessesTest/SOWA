using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SW.DM;

namespace SW.DAL.EntityConfigurations
{
    internal class BillMasterEntityConfiguration : IEntityTypeConfiguration<BillMaster>
    {
        public void Configure(EntityTypeBuilder<BillMaster> entity)
        {
            entity.HasIndex(e => new { e.CustomerType, e.CustomerId }, "IX_CustomerType_CustomerID");

            entity.HasIndex(e => e.TransactionId, "IX_TransactionID");

            entity.HasIndex(e => e.CustomerId, "KOW14");

            entity.Property(e => e.AddDateTime).HasColumnType("datetime");

            entity.Property(e => e.AddToi).HasMaxLength(255);

            entity.Property(e => e.BillMessage).HasMaxLength(100);

            entity.Property(e => e.BillingAddressCityStateZip).HasMaxLength(100);

            entity.Property(e => e.BillingAddressStreet).HasMaxLength(100);

            entity.Property(e => e.BillingName).HasMaxLength(100);

            entity.Property(e => e.BillingPeriodBegDate).HasColumnType("datetime");

            entity.Property(e => e.BillingPeriodEndDate).HasColumnType("datetime");

            entity.Property(e => e.ChgDateTime).HasColumnType("datetime");

            entity.Property(e => e.ChgToi).HasMaxLength(255);

            entity.Property(e => e.ContainerCharges).HasColumnType("decimal(18, 2)");

            entity.Property(e => e.ContractCharge).HasColumnType("decimal(18, 2)");

            entity.Property(e => e.CustomerId).HasColumnName("CustomerID");

            entity.Property(e => e.CustomerType).HasMaxLength(128);

            entity.Property(e => e.DelDateTime).HasColumnType("datetime");

            entity.Property(e => e.DelToi).HasMaxLength(255);

            entity.Property(e => e.PastDueAmt).HasColumnType("decimal(18, 2)");

            entity.Property(e => e.TransactionId).HasColumnName("TransactionID");

            entity.HasOne(d => d.Transaction)
                .WithMany(p => p.BillMasters)
                .HasForeignKey(d => d.TransactionId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_dbo.BillMasters_dbo.Transactions_TransactionID");

            entity.HasOne(d => d.Customer)
                .WithMany(p => p.BillMasters)
                .HasForeignKey(d => new { d.CustomerType, d.CustomerId })
                .HasConstraintName("FK_dbo.BillMasters_dbo.Customers_CustomerType_CustomerID");

            entity.Property(e => e.BillMasterId).ValueGeneratedOnAdd();
            entity.HasKey(e => e.BillMasterId);
        }
    }
}
