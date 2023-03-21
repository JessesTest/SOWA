using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SW.DM;

namespace SW.DAL.EntityConfigurations
{
    internal class PaymentPlanDetailEntityConfiguration : IEntityTypeConfiguration<PaymentPlanDetail>
    {
        public void Configure(EntityTypeBuilder<PaymentPlanDetail> entity)
        {
            entity.HasIndex(e => e.PaymentPlanId, "IX_PaymentPlanId");

            entity.Property(e => e.AddDateTime).HasColumnType("datetime");

            entity.Property(e => e.AddToi).HasMaxLength(255);

            entity.Property(e => e.Amount).HasColumnType("decimal(18, 2)");

            entity.Property(e => e.BillDate).HasColumnType("datetime");

            entity.Property(e => e.ChgDateTime).HasColumnType("datetime");

            entity.Property(e => e.ChgToi).HasMaxLength(255);

            entity.Property(e => e.DelDateTime).HasColumnType("datetime");

            entity.Property(e => e.DelToi).HasMaxLength(255);

            entity.Property(e => e.DueDate).HasColumnType("datetime");

            entity.Property(e => e.PaymentTotal).HasColumnType("decimal(18, 2)");

            entity.HasOne(d => d.PaymentPlan)
                .WithMany(p => p.Details)
                .HasForeignKey(d => d.PaymentPlanId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_dbo.PaymentPlanDetails_dbo.PaymentPlans_PaymentPlanId");

            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            entity.HasKey(e => e.Id);
        }
    }
}
