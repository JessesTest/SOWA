using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SW.DM;

namespace SW.DAL.EntityConfigurations
{
    internal class BillContainerDetailEntityConfiguration : IEntityTypeConfiguration<BillContainerDetail>
    {
        public void Configure(EntityTypeBuilder<BillContainerDetail> entity)
        {
            entity.HasIndex(e => e.BillServiceAddressId, "IX_BillServiceAddressId");

            entity.HasIndex(e => e.ContainerId, "IX_ContainerId");

            entity.Property(e => e.AddDateTime).HasColumnType("datetime");

            entity.Property(e => e.AddToi).HasMaxLength(255);

            entity.Property(e => e.BillingSize).HasColumnType("decimal(18, 2)");

            entity.Property(e => e.ChgDateTime).HasColumnType("datetime");

            entity.Property(e => e.ChgToi).HasMaxLength(255);

            entity.Property(e => e.ContainerCancelDate).HasColumnType("datetime");

            entity.Property(e => e.ContainerCharge).HasColumnType("decimal(18, 2)");

            entity.Property(e => e.ContainerDescription).HasMaxLength(100);

            entity.Property(e => e.ContainerEffectiveDate).HasColumnType("datetime");

            entity.Property(e => e.ContainerType).HasMaxLength(1);

            entity.Property(e => e.DaysProratedMessage).HasMaxLength(100);

            entity.Property(e => e.DaysService).HasMaxLength(6);

            entity.Property(e => e.DelDateTime).HasColumnType("datetime");

            entity.Property(e => e.DelToi).HasMaxLength(255);

            entity.Property(e => e.Partial).HasColumnType("decimal(18, 2)");

            entity.Property(e => e.RateAmount).HasColumnType("decimal(18, 2)");

            entity.Property(e => e.RateDescription).HasMaxLength(100);

            entity.HasOne(d => d.BillServiceAddress)
                .WithMany(p => p.BillContainerDetails)
                .HasForeignKey(d => d.BillServiceAddressId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_dbo.BillContainerDetails_dbo.BillServiceAddresses_BillServiceAddressId");

            entity.HasOne(d => d.Container)
                .WithMany(p => p.BillContainerDetails)
                .HasForeignKey(d => d.ContainerId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_dbo.BillContainerDetails_dbo.Containers_ContainerId");

            entity.Property(e => e.BillContainerDetailId).ValueGeneratedOnAdd();
            entity.HasKey(e => e.BillContainerDetailId);
        }
    }
}
