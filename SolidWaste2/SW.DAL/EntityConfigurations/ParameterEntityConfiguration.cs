using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SW.DM;

namespace SW.DAL.EntityConfigurations
{
    internal class ParameterEntityConfiguration : IEntityTypeConfiguration<Parameter>
    {
        public void Configure(EntityTypeBuilder<Parameter> entity)
        {
            entity.HasKey(e => new { e.FormulaId, e.ParameterId })
                .HasName("PK_dbo.Parameters");

            entity.HasIndex(e => e.FormulaId, "IX_FormulaID");

            entity.Property(e => e.FormulaId).HasColumnName("FormulaID");

            entity.Property(e => e.ParameterId)
                .HasMaxLength(128)
                .HasColumnName("ParameterID");

            entity.Property(e => e.AddDateTime).HasColumnType("datetime");

            entity.Property(e => e.AddToi).HasMaxLength(255);

            entity.Property(e => e.ChgDateTime).HasColumnType("datetime");

            entity.Property(e => e.ChgToi).HasMaxLength(255);

            entity.Property(e => e.DelDateTime).HasColumnType("datetime");

            entity.Property(e => e.DelToi).HasMaxLength(255);

            entity.Property(e => e.Name).HasMaxLength(255);

            entity.Property(e => e.Value).HasColumnType("decimal(18, 2)");

            entity.HasOne(d => d.Formula)
                .WithMany(p => p.Parameters)
                .HasForeignKey(d => d.FormulaId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_dbo.Parameters_dbo.Formulae_FormulaID");
        }
    }
}
