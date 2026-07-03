using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using LaundrySaas.Domain.Identity;
using LaundrySaas.Domain.Organization;
using LaundrySaas.Domain.POS;
using LaundrySaas.Domain.Sales;
using LaundrySaas.Domain.Billing;
using LaundrySaas.Infrastructure.Authentication;
using LaundrySaas.Infrastructure.Persistence.Audit;
using LaundrySaas.SharedKernel.MultiTenancy;
using LaundrySaas.SharedKernel.Primitives;

namespace LaundrySaas.Infrastructure.Persistence;

public class ApplicationDbContext : DbContext
{
    private readonly ITenantProvider _tenantProvider;

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options, ITenantProvider tenantProvider)
        : base(options)
    {
        _tenantProvider = tenantProvider;
    }

    // Identity
    public DbSet<User> Users => Set<User>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<Permission> Permissions => Set<Permission>();
    public DbSet<RolePermission> RolePermissions => Set<RolePermission>();
    public DbSet<UserSession> UserSessions => Set<UserSession>();

    // Organization
    public DbSet<Tenant> Tenants => Set<Tenant>();
    public DbSet<Branch> Branches => Set<Branch>();
    public DbSet<Department> Departments => Set<Department>();
    public DbSet<EmployeeAssignment> EmployeeAssignments => Set<EmployeeAssignment>();

    // POS
    public DbSet<Pos> Poses => Set<Pos>();
    public DbSet<Shift> Shifts => Set<Shift>();
    public DbSet<CashDrawer> CashDrawers => Set<CashDrawer>();
    public DbSet<CashMovement> CashMovements => Set<CashMovement>();

    // Sales
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();
    public DbSet<PaymentAllocation> PaymentAllocations => Set<PaymentAllocation>();
    public DbSet<Promotion> Promotions => Set<Promotion>();
    public DbSet<OrderDiscount> OrderDiscounts => Set<OrderDiscount>();
    public DbSet<OrderTax> OrderTaxes => Set<OrderTax>();
    public DbSet<Domain.Sales.Invoice> OrderInvoices => Set<Domain.Sales.Invoice>();

    // Billing
    public DbSet<PaymentMethod> PaymentMethods => Set<PaymentMethod>();
    public DbSet<PaymentGateway> PaymentGateways => Set<PaymentGateway>();
    public DbSet<Domain.Billing.Invoice> SaaSInvoices => Set<Domain.Billing.Invoice>();
    public DbSet<InvoiceItem> InvoiceItems => Set<InvoiceItem>();
    public DbSet<InvoicePayment> InvoicePayments => Set<InvoicePayment>();
    public DbSet<Subscription> Subscriptions => Set<Subscription>();
    public DbSet<SubscriptionPlan> SubscriptionPlans => Set<SubscriptionPlan>();
    public DbSet<SubscriptionUsage> SubscriptionUsages => Set<SubscriptionUsage>();

    // Audit
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    public Guid CurrentTenantId => _tenantProvider.TenantId ?? Guid.Empty;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Apply all configurations from this assembly
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);

        // Configure Global Query Filters for all multi-tenant entities dynamically
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (typeof(IMustHaveTenant).IsAssignableFrom(entityType.ClrType))
            {
                modelBuilder.Entity(entityType.ClrType)
                    .HasQueryFilter(CreateTenantFilterExpression(entityType.ClrType));
            }
        }
    }

    private System.Linq.Expressions.LambdaExpression CreateTenantFilterExpression(Type type)
    {
        var parameter = System.Linq.Expressions.Expression.Parameter(type, "e");
        var property = System.Linq.Expressions.Expression.Property(parameter, nameof(IMustHaveTenant.TenantId));
        var currentTenantIdProperty = System.Linq.Expressions.Expression.Property(
            System.Linq.Expressions.Expression.Constant(this),
            nameof(CurrentTenantId));

        var compare = System.Linq.Expressions.Expression.Equal(property, currentTenantIdProperty);
        return System.Linq.Expressions.Expression.Lambda(compare, parameter);
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
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
                    // Soft delete
                    entry.State = EntityState.Modified;
                    entry.Entity.DeletedAt = utcNow;
                    break;
            }
        }

        return await base.SaveChangesAsync(cancellationToken);
    }
}
