using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SW.DM;

namespace SW.DAL.EntityConfigurations
{
    internal class TransactionEntityConfiguration : IEntityTypeConfiguration<Transaction>
    {
        public void Configure(EntityTypeBuilder<Transaction> entity)
        {
            entity.HasIndex(e => e.AssociatedTransactionId, "IX_AssociatedTransactionId");

            entity.HasIndex(e => e.ContainerId, "IX_ContainerID");

            entity.HasIndex(e => new { e.CustomerType, e.CustomerId }, "IX_CustomerType_CustomerID");

            entity.HasIndex(e => e.ServiceAddressId, "IX_ServiceAddressID");

            entity.HasIndex(e => e.TransactionCodeId, "IX_TransactionCodeId");

            entity.HasIndex(e => e.TransactionHoldingId, "IX_TransactionHoldingId");

            entity.HasIndex(e => new { e.CustomerId, e.DeleteFlag, e.AddDateTime }, "KOW16")
                .HasFillFactor(90);

            entity.Property(e => e.AddDateTime).HasColumnType("datetime");

            entity.Property(e => e.AddToi).HasMaxLength(255);

            entity.Property(e => e.ChgDateTime).HasColumnType("datetime");

            entity.Property(e => e.ChgToi).HasMaxLength(255);

            entity.Property(e => e.CollectionsAmount).HasColumnType("decimal(18, 2)");

            entity.Property(e => e.CollectionsBalance).HasColumnType("decimal(18, 2)");

            entity.Property(e => e.Comment).HasMaxLength(100);

            entity.Property(e => e.ContainerId).HasColumnName("ContainerID");

            entity.Property(e => e.CounselorsAmount).HasColumnType("decimal(18, 2)");

            entity.Property(e => e.CounselorsBalance).HasColumnType("decimal(18, 2)");

            entity.Property(e => e.CustomerId).HasColumnName("CustomerID");

            entity.Property(e => e.CustomerType).HasMaxLength(128);

            entity.Property(e => e.DelDateTime).HasColumnType("datetime");

            entity.Property(e => e.DelToi).HasMaxLength(255);

            entity.Property(e => e.Partial).HasColumnType("decimal(18, 2)");

            entity.Property(e => e.ServiceAddressId).HasColumnName("ServiceAddressID");

            entity.Property(e => e.TransactionAmt).HasColumnType("decimal(18, 2)");

            entity.Property(e => e.TransactionBalance).HasColumnType("decimal(18, 2)");

            entity.Property(e => e.UncollectableAmount).HasColumnType("decimal(18, 2)");

            entity.Property(e => e.UncollectableBalance).HasColumnType("decimal(18, 2)");

            entity.Property(e => e.WorkOrder).HasMaxLength(16);

            entity.HasOne(d => d.AssociatedTransaction)
                .WithMany(p => p.InverseAssociatedTransaction)
                .HasForeignKey(d => d.AssociatedTransactionId)
                .HasConstraintName("FK_dbo.Transactions_dbo.Transactions_AssociatedTransactionId");

            entity.HasOne(d => d.Container)
                .WithMany(p => p.Transactions)
                .HasForeignKey(d => d.ContainerId)
                .HasConstraintName("FK_dbo.Transactions_dbo.Containers_ContainerID");

            entity.HasOne(d => d.ServiceAddress)
                .WithMany(p => p.Transactions)
                .HasForeignKey(d => d.ServiceAddressId)
                .HasConstraintName("FK_dbo.Transactions_dbo.ServiceAddresses_ServiceAddressID");

            entity.HasOne(d => d.TransactionCode)
                .WithMany(p => p.Transactions)
                .HasForeignKey(d => d.TransactionCodeId)
                .HasConstraintName("FK_dbo.Transactions_dbo.TransactionCodes_TransactionCodeId");

            entity.HasOne(d => d.TransactionHolding)
                .WithMany(p => p.Transactions)
                .HasForeignKey(d => d.TransactionHoldingId)
                .HasConstraintName("FK_dbo.Transactions_dbo.TransactionHoldings_TransactionHoldingId");

            entity.HasOne(d => d.Customer)
                .WithMany(p => p.Transactions)
                .HasForeignKey(d => new { d.CustomerType, d.CustomerId })
                .HasConstraintName("FK_dbo.Transactions_dbo.Customers_CustomerType_CustomerID");

            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            entity.HasKey(e => e.Id);
        }
    }
}
