using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SW.DM;

namespace SW.DAL.EntityConfigurations
{
    internal class WorkOrderEntityConfiguration : IEntityTypeConfiguration<WorkOrder>
    {
        public void Configure(EntityTypeBuilder<WorkOrder> entity)
        {
            entity.HasIndex(e => e.ContainerId, "IX_ContainerId");

            entity.HasIndex(e => new { e.CustomerType, e.CustomerId }, "IX_CustomerType_CustomerId");

            entity.HasIndex(e => e.ServiceAddressId, "IX_ServiceAddressId");

            entity.HasIndex(e => new { e.ResolveDate, e.DelFlag, e.CustomerAddress }, "KOW15");

            entity.Property(e => e.AddToi).HasMaxLength(64);

            entity.Property(e => e.ChgToi).HasMaxLength(64);

            entity.Property(e => e.ContainerCode).HasMaxLength(1);

            entity.Property(e => e.ContainerRoute).HasMaxLength(5);

            entity.Property(e => e.ContainerSize).HasColumnType("decimal(3, 1)");

            entity.Property(e => e.CustomerAddress).HasMaxLength(128);

            entity.Property(e => e.CustomerName).HasMaxLength(64);

            entity.Property(e => e.CustomerType).HasMaxLength(128);

            entity.Property(e => e.DelToi).HasMaxLength(64);

            entity.Property(e => e.DriverInitials).HasMaxLength(4);

            entity.Property(e => e.RepairsNeeded).HasMaxLength(256);

            entity.Property(e => e.ResolutionNotes).HasMaxLength(256);

            entity.Property(e => e.ResolveDate).HasColumnType("date");

            entity.Property(e => e.TransDate).HasColumnType("date");

            entity.HasOne(d => d.Container)
                .WithMany(p => p.WorkOrders)
                .HasForeignKey(d => d.ContainerId)
                .HasConstraintName("FK_dbo.WorkOrders_dbo.Containers_ContainerId");

            entity.HasOne(d => d.ServiceAddress)
                .WithMany(p => p.WorkOrders)
                .HasForeignKey(d => d.ServiceAddressId)
                .HasConstraintName("FK_dbo.WorkOrders_dbo.ServiceAddresses_ServiceAddressId");

            entity.HasOne(d => d.Customer)
                .WithMany(p => p.WorkOrders)
                .HasForeignKey(d => new { d.CustomerType, d.CustomerId })
                .HasConstraintName("FK_dbo.WorkOrders_dbo.Customers_CustomerType_CustomerId");

            entity.Property(e => e.WorkOrderId).ValueGeneratedOnAdd();
            entity.HasKey(e => e.WorkOrderId);
        }
    }
}
