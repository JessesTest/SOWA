using Identity.DM;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Identity.DAL.EntityConfigurations;

internal class AspNetUserEntityConfiguration : IEntityTypeConfiguration<AspNetUser>
{
    public void Configure(EntityTypeBuilder<AspNetUser> builder)
    {
        builder.HasIndex(e => e.UserName, "UserNameIndex")
                .IsUnique();

        builder.HasIndex(e => e.NormalizedUserName, "NormalizedUserNameIndex")
                    .IsUnique()
                    .HasFilter("([NormalizedUserName] IS NOT NULL)");

        builder.Property(e => e.Id).HasMaxLength(128);

        builder.Property(e => e.Email).HasMaxLength(256);

        builder.Property(e => e.NormalizedEmail).HasMaxLength(256);

        builder.Property(e => e.LockoutEndDateUtc).HasColumnType("datetime");

        builder.Property(e => e.NormalizedUserName).HasMaxLength(256);

        builder.Property(e => e.UserName).HasMaxLength(256);

        builder.HasMany(d => d.Roles)
            .WithMany(p => p.Users)
            .UsingEntity<Dictionary<string, object>>(
                "AspNetUserRole",
                l => l.HasOne<AspNetRole>().WithMany().HasForeignKey("RoleId").HasConstraintName("FK_dbo.AspNetUserRoles_dbo.AspNetRoles_RoleId"),
                r => r.HasOne<AspNetUser>().WithMany().HasForeignKey("UserId").HasConstraintName("FK_dbo.AspNetUserRoles_dbo.AspNetUsers_UserId"),
                j =>
                {
                    j.HasKey("UserId", "RoleId").HasName("PK_dbo.AspNetUserRoles");

                    j.ToTable("AspNetUserRoles");

                    j.HasIndex(new[] { "RoleId" }, "IX_RoleId");

                    j.HasIndex(new[] { "UserId" }, "IX_UserId");

                    j.IndexerProperty<string>("UserId").HasMaxLength(128);

                    j.IndexerProperty<string>("RoleId").HasMaxLength(128);
                });
    }
}
