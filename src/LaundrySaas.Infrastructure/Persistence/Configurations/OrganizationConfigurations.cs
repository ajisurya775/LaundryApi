using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using LaundrySaas.Domain.Organization;
using LaundrySaas.Domain.Identity;

namespace LaundrySaas.Infrastructure.Persistence.Configurations;

public class TenantConfiguration : IEntityTypeConfiguration<Tenant>
{
    public void Configure(EntityTypeBuilder<Tenant> builder)
    {
        builder.HasKey(t => t.Id);
        builder.Property(t => t.CountryCode).HasMaxLength(10).IsRequired(false);
        builder.Property(t => t.PhoneNumber).HasMaxLength(30).IsRequired(false);
    }
}

public class BranchConfiguration : IEntityTypeConfiguration<Branch>
{
    public void Configure(EntityTypeBuilder<Branch> builder)
    {
        builder.HasKey(b => b.Id);
    }
}

public class DepartmentConfiguration : IEntityTypeConfiguration<Department>
{
    public void Configure(EntityTypeBuilder<Department> builder)
    {
        builder.HasKey(d => d.Id);
        builder.HasOne<Branch>()
            .WithMany()
            .HasForeignKey(d => d.BranchId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class EmployeeAssignmentConfiguration : IEntityTypeConfiguration<EmployeeAssignment>
{
    public void Configure(EntityTypeBuilder<EmployeeAssignment> builder)
    {
        builder.HasKey(ea => ea.Id);
        builder.HasIndex(ea => new { ea.UserId, ea.BranchId, ea.RoleId, ea.PosId }).IsUnique();

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(ea => ea.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne<Branch>()
            .WithMany()
            .HasForeignKey(ea => ea.BranchId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired(false);

        builder.HasOne<Role>()
            .WithMany()
            .HasForeignKey(ea => ea.RoleId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
