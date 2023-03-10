using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SW.DM;

namespace SW.DAL.EntityConfigurations
{
    internal class BillBlobEntityConfiguration : IEntityTypeConfiguration<BillBlobs>
    {
        public void Configure(EntityTypeBuilder<BillBlobs> entity)
        {
            entity.HasIndex(e => e.TransactionId, "KOW32");

            entity.HasIndex(e => new { e.CustomerId, e.BegDateTime, e.EndDateTime, e.DelFlag }, "KOW4");

            entity.Property(e => e.AddToi).HasMaxLength(64);

            entity.Property(e => e.BegDateTime).HasColumnType("datetime");

            entity.Property(e => e.ChgToi).HasMaxLength(64);

            entity.Property(e => e.CustomerId).HasColumnName("CustomerID");

            entity.Property(e => e.DelToi).HasMaxLength(64);

            entity.Property(e => e.EndDateTime).HasColumnType("datetime");

            entity.Property(e => e.TransactionId).HasColumnName("TransactionID");

            entity.Property(e => e.BillBlobId).ValueGeneratedOnAdd();
            entity.HasKey(e => e.BillBlobId);
        }
    }
}
