using Microsoft.EntityFrameworkCore;
using LaundrySaas.Domain.MultiTenancy;
using LaundrySaas.Domain.Users;
using LaundrySaas.Application.Abstractions;
using LaundrySaas.SharedKernel.Interfaces;
using LaundrySaas.SharedKernel.Primitives;

namespace LaundrySaas.Infrastructure.Data;

public class ApplicationDbContext : DbContext
{
    private readonly ITenantProvider _tenantProvider;

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options, ITenantProvider tenantProvider)
        : base(options)
    {
        _tenantProvider = tenantProvider;
    }

    public DbSet<Tenant> Tenants => Set<Tenant>();
    public DbSet<Branch> Branches => Set<Branch>();
    public DbSet<User> Users => Set<User>();
    public DbSet<UserBranch> UserBranches => Set<UserBranch>();
    public DbSet<MenuItem> MenuItems => Set<MenuItem>();
    public DbSet<RoleMenuAccess> RoleMenuAccesses => Set<RoleMenuAccess>();
    public DbSet<Pos> Poses => Set<Pos>();
    public DbSet<PaymentMethod> PaymentMethods => Set<PaymentMethod>();
    public DbSet<PosPaymentMethod> PosPaymentMethods => Set<PosPaymentMethod>();

    public Guid CurrentTenantId => _tenantProvider.TenantId ?? Guid.Empty;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure Primary Keys & Multi-Tenant Global Query Filters
        modelBuilder.Entity<Tenant>().HasKey(t => t.Id);

        modelBuilder.Entity<Branch>(builder =>
        {
            builder.HasKey(b => b.Id);
            builder.HasQueryFilter(b => b.TenantId == CurrentTenantId);

            // Configure backing field for PosList
            builder.Navigation(b => b.PosList)
                .UsePropertyAccessMode(PropertyAccessMode.Field);
        });

        modelBuilder.Entity<Pos>(builder =>
        {
            builder.HasKey(p => p.Id);
            builder.HasQueryFilter(p => p.TenantId == CurrentTenantId);

            // Configure backing field for payment methods
            builder.Navigation(p => p.PaymentMethods)
                .UsePropertyAccessMode(PropertyAccessMode.Field);
        });

        modelBuilder.Entity<PaymentMethod>(builder =>
        {
            builder.HasKey(pm => pm.Id);
            builder.HasQueryFilter(pm => pm.TenantId == CurrentTenantId);
        });

        modelBuilder.Entity<PosPaymentMethod>(builder =>
        {
            builder.HasKey(ppm => ppm.Id);
            builder.HasQueryFilter(ppm => ppm.TenantId == CurrentTenantId);
        });

        modelBuilder.Entity<User>(builder =>
        {
            builder.HasKey(u => u.Id);
            builder.HasQueryFilter(u => u.TenantId == CurrentTenantId);

            // Configure assigned branches backing field
            builder.Navigation(u => u.AssignedBranches)
                .UsePropertyAccessMode(PropertyAccessMode.Field);
        });

        modelBuilder.Entity<UserBranch>(builder =>
        {
            builder.HasKey(ub => ub.Id);
            builder.HasQueryFilter(ub => ub.TenantId == CurrentTenantId);
        });

        modelBuilder.Entity<MenuItem>(builder =>
        {
            builder.HasKey(m => m.Id);
        });

        modelBuilder.Entity<RoleMenuAccess>(builder =>
        {
            builder.HasKey(r => r.Id);
            builder.HasQueryFilter(r => r.TenantId == CurrentTenantId);
        });
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // Auto-populate audit fields and tenant id
        var utcNow = DateTime.UtcNow;
        foreach (var entry in ChangeTracker.Entries<Entity>())
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Entity.CreatedAt = utcNow;
                    if (entry.Entity is IMustHaveTenant)
                    {
                        var tenantIdProperty = entry.Property("TenantId");
                        if (CurrentTenantId != Guid.Empty)
                        {
                            tenantIdProperty.CurrentValue = CurrentTenantId;
                        }
                        else if (tenantIdProperty.CurrentValue is null || (Guid)tenantIdProperty.CurrentValue == Guid.Empty)
                        {
                            tenantIdProperty.CurrentValue = Guid.Empty;
                        }
                    }
                    break;
                case EntityState.Modified:
                    entry.Entity.UpdatedAt = utcNow;
                    break;
                case EntityState.Deleted:
                    // Soft-delete: mark as deleted instead of physical removal
                    entry.State = EntityState.Modified;
                    entry.Entity.DeletedAt = utcNow;
                    break;
            }
        }

        return await base.SaveChangesAsync(cancellationToken);
    }
}
