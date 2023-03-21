using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SW.DM;

namespace SW.DAL.EntityConfigurations
{
    internal class KanPayEntityConfiguration : IEntityTypeConfiguration<KanPay>
    {
        public void Configure(EntityTypeBuilder<KanPay> entity)
        {
            entity.Property(e => e.KanPayId).HasColumnName("KanPayID");

            entity.Property(e => e.KanPayAddDateTime).HasColumnType("datetime");

            entity.Property(e => e.KanPayAddToi).HasMaxLength(255);

            entity.Property(e => e.KanPayAmount).HasMaxLength(255);

            entity.Property(e => e.KanPayChgDateTime).HasColumnType("datetime");

            entity.Property(e => e.KanPayChgToi).HasMaxLength(255);

            entity.Property(e => e.KanPayCid).HasMaxLength(255);

            entity.Property(e => e.KanPayCustomerType).HasMaxLength(255);

            entity.Property(e => e.KanPayDelDateTime).HasColumnType("datetime");

            entity.Property(e => e.KanPayDelToi).HasMaxLength(255);

            entity.Property(e => e.KanPayIfasObjectCode).HasMaxLength(255);

            entity.Property(e => e.KanPayTokenId)
                .HasMaxLength(255)
                .HasColumnName("KanPayTokenID");

            entity.Property(e => e.KanPayId).ValueGeneratedOnAdd();
            entity.HasKey(e => e.KanPayId);
        }
    }
}
