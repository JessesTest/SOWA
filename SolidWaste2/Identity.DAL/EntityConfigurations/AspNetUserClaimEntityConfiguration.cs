using Identity.DM;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Identity.DAL.EntityConfigurations;

internal class AspNetUserClaimEntityConfiguration : IEntityTypeConfiguration<AspNetUserClaim>
{
    public void Configure(EntityTypeBuilder<AspNetUserClaim> builder)
    {
        builder.HasIndex(e => e.UserId, "IX_UserId");

        builder.Property(e => e.UserId).HasMaxLength(128);

        builder.HasOne(d => d.User)
            .WithMany(p => p.AspNetUserClaims)
            .HasForeignKey(d => d.UserId)
            .HasConstraintName("FK_dbo.AspNetUserClaims_dbo.AspNetUsers_UserId");
    }
}
