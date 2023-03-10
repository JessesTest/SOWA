using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SW.DM;

namespace SW.DAL.EntityConfigurations
{
    internal class TransactionCodeEntityConfiguration : IEntityTypeConfiguration<TransactionCode>
    {
        public void Configure(EntityTypeBuilder<TransactionCode> entity)
        {
            entity.Property(e => e.TransactionCodeId).HasColumnName("TransactionCodeID");

            entity.Property(e => e.AccountType).HasMaxLength(8);

            entity.Property(e => e.AddDateTime).HasColumnType("datetime");

            entity.Property(e => e.AddToi).HasMaxLength(255);

            entity.Property(e => e.ChgDateTime).HasColumnType("datetime");

            entity.Property(e => e.ChgToi).HasMaxLength(255);

            entity.Property(e => e.Code).HasMaxLength(5);

            entity.Property(e => e.CollectionsBalanceSign).HasMaxLength(8);

            entity.Property(e => e.CounselorsBalanceSign).HasMaxLength(8);

            entity.Property(e => e.DelDateTime).HasColumnType("datetime");

            entity.Property(e => e.DelToi).HasMaxLength(255);

            entity.Property(e => e.Description).HasMaxLength(50);

            entity.Property(e => e.Group).HasMaxLength(8);

            entity.Property(e => e.Security).HasMaxLength(32);

            entity.Property(e => e.TransactionSign).HasMaxLength(8);

            entity.Property(e => e.UncollectableBalanceSign).HasMaxLength(8);

            entity.Property(e => e.TransactionCodeId).ValueGeneratedOnAdd();
            entity.HasKey(e => e.TransactionCodeId);
        }
    }
}
