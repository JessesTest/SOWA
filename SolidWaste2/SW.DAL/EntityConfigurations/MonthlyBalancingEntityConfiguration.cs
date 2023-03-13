using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SW.DM;

namespace SW.DAL.EntityConfigurations
{
    internal class MonthlyBalancingEntityConfiguration : IEntityTypeConfiguration<MonthlyBalancing>
    {
        public void Configure(EntityTypeBuilder<MonthlyBalancing> entity)
        {
            entity.Property(e => e.MonthlyBalancingId).HasColumnName("MonthlyBalancingID");

            entity.Property(e => e.AccountType).HasMaxLength(16);

            entity.Property(e => e.AccountTypeReceivable).HasColumnType("decimal(18, 2)");

            entity.Property(e => e.AccountTypeRevenue).HasColumnType("decimal(18, 2)");

            entity.Property(e => e.AddDateTime).HasColumnType("datetime");

            entity.Property(e => e.BeginningBalance).HasColumnType("decimal(18, 2)");

            entity.Property(e => e.ChgDateTime).HasColumnType("datetime");

            entity.Property(e => e.DelDateTime).HasColumnType("datetime");

            entity.Property(e => e.ObjectDescription).HasMaxLength(32);

            entity.Property(e => e.TotalReceivable).HasColumnType("decimal(18, 2)");

            entity.Property(e => e.TotalRevenue).HasColumnType("decimal(18, 2)");

            entity.Property(e => e.TransactionAmount).HasColumnType("decimal(18, 2)");

            entity.Property(e => e.TransactionCode).HasMaxLength(4);

            entity.Property(e => e.TransactionCodeDescription).HasMaxLength(64);

            entity.Property(e => e.MonthlyBalancingId).ValueGeneratedOnAdd();
            entity.HasKey(e => e.MonthlyBalancingId);
        }
    }
}
