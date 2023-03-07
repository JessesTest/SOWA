using Identity.DM;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Identity.DAL.EntityConfigurations;

internal class AspNetUserLoginEntityConfiguration : IEntityTypeConfiguration<AspNetUserLogin>
{
    public void Configure(EntityTypeBuilder<AspNetUserLogin> builder)
    {
        builder.HasKey(e => new { e.LoginProvider, e.ProviderKey, e.UserId })
                .HasName("PK_dbo.AspNetUserLogins");

        builder.HasIndex(e => e.UserId, "IX_UserId");

        builder.Property(e => e.LoginProvider).HasMaxLength(128);

        builder.Property(e => e.ProviderKey).HasMaxLength(128);

        builder.Property(e => e.UserId).HasMaxLength(128);

        builder.HasOne(d => d.User)
            .WithMany(p => p.AspNetUserLogins)
            .HasForeignKey(d => d.UserId)
            .HasConstraintName("FK_dbo.AspNetUserLogins_dbo.AspNetUsers_UserId");
    }
}
