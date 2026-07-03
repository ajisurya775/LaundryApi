using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using LaundrySaas.Domain.Identity;
using LaundrySaas.Infrastructure.Authentication;

namespace LaundrySaas.Infrastructure.Persistence.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.HasKey(u => u.Id);
        // Note: Global query filters will be set dynamically or in DbContext if needed, 
        // but we can also set them here
    }
}

public class RoleConfiguration : IEntityTypeConfiguration<Role>
{
    public void Configure(EntityTypeBuilder<Role> builder)
    {
        builder.HasKey(r => r.Id);
        builder.Property(r => r.Code).HasMaxLength(50);
        builder.Navigation(r => r.RolePermissions).UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}

public class PermissionConfiguration : IEntityTypeConfiguration<Permission>
{
    public void Configure(EntityTypeBuilder<Permission> builder)
    {
        builder.HasKey(p => p.Id);
        builder.HasIndex(p => p.Key).IsUnique();
        builder.Property(p => p.Key).HasMaxLength(100);
        builder.Property(p => p.Category).HasMaxLength(50);
    }
}

public class RolePermissionConfiguration : IEntityTypeConfiguration<RolePermission>
{
    public void Configure(EntityTypeBuilder<RolePermission> builder)
    {
        builder.HasKey(rp => rp.Id);
        builder.HasIndex(rp => new { rp.RoleId, rp.PermissionId }).IsUnique();

        builder.HasOne<Role>()
            .WithMany(r => r.RolePermissions)
            .HasForeignKey(rp => rp.RoleId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne<Permission>()
            .WithMany()
            .HasForeignKey(rp => rp.PermissionId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public class UserSessionConfiguration : IEntityTypeConfiguration<UserSession>
{
    public void Configure(EntityTypeBuilder<UserSession> builder)
    {
        builder.HasKey(us => us.Id);
        builder.Property(us => us.RefreshToken).HasMaxLength(500);
        builder.Property(us => us.Device).HasMaxLength(100).IsRequired(false);
        builder.Property(us => us.Browser).HasMaxLength(100).IsRequired(false);
        builder.Property(us => us.IPAddress).HasMaxLength(50).IsRequired(false);

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(us => us.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
