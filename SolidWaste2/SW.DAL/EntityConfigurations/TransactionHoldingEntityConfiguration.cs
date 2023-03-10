using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SW.DM;

namespace SW.DAL.EntityConfigurations
{
    internal class TransactionHoldingEntityConfiguration : IEntityTypeConfiguration<TransactionHolding>
    {
        public void Configure(EntityTypeBuilder<TransactionHolding> entity)
        {
            entity.HasIndex(e => e.ContainerId, "IX_ContainerId");

            entity.HasIndex(e => new { e.CustomerType, e.CustomerId }, "IX_CustomerType_CustomerID");

            entity.HasIndex(e => e.ServiceAddressId, "IX_ServiceAddressId");

            entity.HasIndex(e => e.TransactionCodeId, "IX_TransactionCodeId");

            entity.HasIndex(e => new { e.Status, e.BatchId, e.DeleteFlag }, "KOW10");

            entity.Property(e => e.AddDateTime).HasColumnType("datetime");

            entity.Property(e => e.AddToi).HasMaxLength(255);

            entity.Property(e => e.Approver).HasMaxLength(255);

            entity.Property(e => e.BatchId).HasColumnName("BatchID");

            entity.Property(e => e.ChgDateTime).HasColumnType("datetime");

            entity.Property(e => e.ChgToi).HasMaxLength(255);

            entity.Property(e => e.Comment).HasMaxLength(100);

            entity.Property(e => e.CustomerId).HasColumnName("CustomerID");

            entity.Property(e => e.CustomerType).HasMaxLength(128);

            entity.Property(e => e.DelDateTime).HasColumnType("datetime");

            entity.Property(e => e.DelToi).HasMaxLength(255);

            entity.Property(e => e.Sender).HasMaxLength(255);

            entity.Property(e => e.Status).HasMaxLength(32);

            entity.Property(e => e.TransactionAmt).HasColumnType("decimal(18, 2)");

            entity.Property(e => e.WorkOrder).HasMaxLength(16);

            entity.HasOne(d => d.Container)
                .WithMany(p => p.TransactionHoldings)
                .HasForeignKey(d => d.ContainerId)
                .HasConstraintName("FK_dbo.TransactionHoldings_dbo.Containers_ContainerId");

            entity.HasOne(d => d.ServiceAddress)
                .WithMany(p => p.TransactionHoldings)
                .HasForeignKey(d => d.ServiceAddressId)
                .HasConstraintName("FK_dbo.TransactionHoldings_dbo.ServiceAddresses_ServiceAddressId");

            entity.HasOne(d => d.TransactionCode)
                .WithMany(p => p.TransactionHoldings)
                .HasForeignKey(d => d.TransactionCodeId)
                .HasConstraintName("FK_dbo.TransactionHoldings_dbo.TransactionCodes_TransactionCodeId");

            entity.HasOne(d => d.Customer)
                .WithMany(p => p.TransactionHoldings)
                .HasForeignKey(d => new { d.CustomerType, d.CustomerId })
                .HasConstraintName("FK_dbo.TransactionHoldings_dbo.Customers_CustomerType_CustomerID");

            entity.Property(e => e.TransactionHoldingId).ValueGeneratedOnAdd();
            entity.HasKey(e => e.TransactionHoldingId);
        }
    }
}
