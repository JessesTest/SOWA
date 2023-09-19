using Identity.DM;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Identity.DAL.EntityConfigurations
{
    internal class AspNetRoleClaimEntityConfiguration : IEntityTypeConfiguration<AspNetRoleClaim>
    {
        public void Configure(EntityTypeBuilder<AspNetRoleClaim> builder)
        {
            builder.HasIndex(e => e.RoleId, "IX_AspNetRoleClaims_RoleId");

            builder.HasOne(d => d.Role)
                .WithMany(p => p.AspNetRoleClaims)
                .HasForeignKey(d => d.RoleId);
        }
    }
}
