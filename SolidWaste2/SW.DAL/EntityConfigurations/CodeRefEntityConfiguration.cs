using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SW.DM;

namespace SW.DAL.EntityConfigurations
{
    internal class CodeRefEntityConfiguration : IEntityTypeConfiguration<CodeRef>
    {
        public void Configure(EntityTypeBuilder<CodeRef> entity)
        {
            entity.HasKey(e => e.CodeId)
                .HasName("PK_dbo.CodeRefs");

            entity.Property(e => e.AddToi).HasMaxLength(64);

            entity.Property(e => e.ChgToi).HasMaxLength(64);

            entity.Property(e => e.Code).HasMaxLength(8);

            entity.Property(e => e.DelToi).HasMaxLength(64);

            entity.Property(e => e.LongDescription).HasMaxLength(48);

            entity.Property(e => e.ShortDescription).HasMaxLength(8);

            entity.Property(e => e.CodeId).ValueGeneratedOnAdd();
        }
    }
}
