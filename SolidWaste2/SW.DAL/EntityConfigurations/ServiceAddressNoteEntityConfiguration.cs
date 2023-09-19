using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SW.DM;

namespace SW.DAL.EntityConfigurations
{
    internal class ServiceAddressNoteEntityConfiguration : IEntityTypeConfiguration<ServiceAddressNote>
    {
        public void Configure(EntityTypeBuilder<ServiceAddressNote> entity)
        {
            entity.HasIndex(e => e.ServiceAddressId, "IX_ServiceAddressId");

            entity.Property(e => e.AddDateTime).HasColumnType("datetime");

            entity.Property(e => e.AddToi).HasMaxLength(255);

            entity.Property(e => e.ChgDateTime).HasColumnType("datetime");

            entity.Property(e => e.ChgToi).HasMaxLength(255);

            entity.Property(e => e.DelDateTime).HasColumnType("datetime");

            entity.Property(e => e.DelToi).HasMaxLength(255);

            entity.Property(e => e.Note).HasMaxLength(1024);

            entity.HasOne(d => d.ServiceAddress)
                .WithMany(p => p.ServiceAddressNotes)
                .HasForeignKey(d => d.ServiceAddressId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_dbo.ServiceAddressNotes_dbo.ServiceAddresses_ServiceAddressId");

            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            entity.HasKey(e => e.Id);
        }
    }
}
