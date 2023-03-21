using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SW.DM;

namespace SW.DAL.EntityConfigurations
{
    internal class TransactionCodeRuleEntityConfiguration : IEntityTypeConfiguration<TransactionCodeRule>
    {
        public void Configure(EntityTypeBuilder<TransactionCodeRule> entity)
        {
            entity.HasIndex(e => e.ContainerCodeId, "IX_ContainerCodeID");

            entity.HasIndex(e => e.ContainerSubtypeId, "IX_ContainerSubtypeID");

            entity.HasIndex(e => e.FormulaId, "IX_FormulaID");

            entity.HasIndex(e => e.TransactionCodeId, "IX_TransactionCodeID");

            entity.HasIndex(e => e.TransactionId, "IX_Transaction_Id");

            entity.Property(e => e.TransactionCodeRuleId).HasColumnName("TransactionCodeRuleID");

            entity.Property(e => e.AddToi).HasMaxLength(64);

            entity.Property(e => e.ChgToi).HasMaxLength(64);

            entity.Property(e => e.ContainerBillingSize).HasColumnType("decimal(18, 2)");

            entity.Property(e => e.ContainerCodeId).HasColumnName("ContainerCodeID");

            entity.Property(e => e.ContainerSubtypeId).HasColumnName("ContainerSubtypeID");

            entity.Property(e => e.DelToi).HasMaxLength(64);

            entity.Property(e => e.Description).HasMaxLength(64);

            entity.Property(e => e.FormulaId).HasColumnName("FormulaID");

            entity.Property(e => e.TransactionCodeId).HasColumnName("TransactionCodeID");

            entity.Property(e => e.TransactionId).HasColumnName("Transaction_Id");

            entity.HasOne(d => d.ContainerCode)
                .WithMany(p => p.TransactionCodeRules)
                .HasForeignKey(d => d.ContainerCodeId)
                .HasConstraintName("FK_dbo.TransactionCodeRules_dbo.ContainerCodes_ContainerCodeID");

            entity.HasOne(d => d.ContainerSubtype)
                .WithMany(p => p.TransactionCodeRules)
                .HasForeignKey(d => d.ContainerSubtypeId)
                .HasConstraintName("FK_dbo.TransactionCodeRules_dbo.ContainerSubtypes_ContainerSubtypeID");

            entity.HasOne(d => d.Formula)
                .WithMany(p => p.TransactionCodeRules)
                .HasForeignKey(d => d.FormulaId)
                .HasConstraintName("FK_dbo.TransactionCodeRules_dbo.Formulae_FormulaID");

            entity.HasOne(d => d.TransactionCode)
                .WithMany(p => p.TransactionCodeRules)
                .HasForeignKey(d => d.TransactionCodeId)
                .HasConstraintName("FK_dbo.TransactionCodeRules_dbo.TransactionCodes_TransactionCodeID");

            entity.HasOne(d => d.Transaction)
                .WithMany(p => p.TransactionCodeRules)
                .HasForeignKey(d => d.TransactionId)
                .HasConstraintName("FK_dbo.TransactionCodeRules_dbo.Transactions_Transaction_Id");

            entity.Property(e => e.TransactionCodeRuleId).ValueGeneratedOnAdd();
            entity.HasKey(e => e.TransactionCodeRuleId);
        }
    }
}
