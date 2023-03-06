using Identity.DM;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Identity.DAL.EntityConfigurations;

internal class AspNetRoleEntityConfiguration : IEntityTypeConfiguration<AspNetRole>
{
    public void Configure(EntityTypeBuilder<AspNetRole> entity)
    {
        entity.HasIndex(e => e.Name, "RoleNameIndex")
                .IsUnique();

        entity.HasIndex(e => e.NormalizedName, "RoleNormalizedNameIndex")
            .IsUnique()
            .HasFilter("([NormalizedName] IS NOT NULL)");

        entity.Property(e => e.Id).HasMaxLength(128);

        entity.Property(e => e.Name).HasMaxLength(256);

        entity.Property(e => e.NormalizedName).HasMaxLength(256);
    }
}
