using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SW.DM;

namespace SW.DAL.EntityConfigurations
{
    internal class PastDueEntityConfiguration : IEntityTypeConfiguration<PastDue>
    {
        public void Configure(EntityTypeBuilder<PastDue> entity)
        {
            entity.HasIndex(e => e.TransactionId, "IX_TransactionId");

            entity.HasIndex(e => new { e.Days, e.ProcessDate, e.DeleteFlag }, "KOW35");

            entity.Property(e => e.AddDateTime).HasColumnType("datetime");

            entity.Property(e => e.AddToi).HasMaxLength(255);

            entity.Property(e => e.Address1).HasMaxLength(255);

            entity.Property(e => e.Address2).HasMaxLength(255);

            entity.Property(e => e.Amount).HasColumnType("decimal(18, 2)");

            entity.Property(e => e.ChgDateTime).HasColumnType("datetime");

            entity.Property(e => e.ChgToi).HasMaxLength(255);

            entity.Property(e => e.DelDateTime).HasColumnType("datetime");

            entity.Property(e => e.DelToi).HasMaxLength(255);

            entity.Property(e => e.Name).HasMaxLength(255);

            entity.Property(e => e.ProcessDate).HasColumnType("datetime");

            entity.HasOne(d => d.Transaction)
                .WithMany(p => p.PastDues)
                .HasForeignKey(d => d.TransactionId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_dbo.PastDues_dbo.Transactions_TransactionId");

            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            entity.HasKey(e => e.Id);
        }
    }
}
