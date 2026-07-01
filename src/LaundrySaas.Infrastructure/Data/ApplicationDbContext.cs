using Microsoft.EntityFrameworkCore;
using LaundrySaas.Domain.MultiTenancy;
using LaundrySaas.Domain.Users;
using LaundrySaas.Domain.Billing;
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

    // Billing DbSets
    public DbSet<CreditBalance> CreditBalances => Set<CreditBalance>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();
    public DbSet<OrderPayment> OrderPayments => Set<OrderPayment>();
    public DbSet<PaymentProvider> PaymentProviders => Set<PaymentProvider>();
    public DbSet<PaymentChannel> PaymentChannels => Set<PaymentChannel>();
    public DbSet<CreditPackage> CreditPackages => Set<CreditPackage>();

    public Guid CurrentTenantId => _tenantProvider.TenantId ?? Guid.Empty;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure Primary Keys & Multi-Tenant Global Query Filters
        modelBuilder.Entity<Tenant>(builder =>
        {
            builder.HasKey(t => t.Id);
            builder.Property(t => t.CountryCode).HasMaxLength(10).IsRequired(false);
            builder.Property(t => t.PhoneNumber).HasMaxLength(30).IsRequired(false);
        });

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

        // Billing configurations
        modelBuilder.Entity<CreditBalance>(builder =>
        {
            builder.HasKey(cb => cb.Id);
            builder.HasQueryFilter(cb => cb.TenantId == CurrentTenantId);
            
            builder.Navigation(cb => cb.Orders)
                .UsePropertyAccessMode(PropertyAccessMode.Field);
        });

        modelBuilder.Entity<Order>(builder =>
        {
            builder.HasKey(o => o.Id);
            builder.HasQueryFilter(o => o.TenantId == CurrentTenantId);
            
            builder.Navigation(o => o.Items)
                .UsePropertyAccessMode(PropertyAccessMode.Field);

            builder.HasOne(o => o.Payment)
                .WithOne()
                .HasForeignKey<OrderPayment>(op => op.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.OwnsOne(o => o.SubTotal, subTotal =>
            {
                subTotal.Property(m => m.Amount).HasColumnName("SubTotalAmount").HasPrecision(18, 2);
                subTotal.Property(m => m.CurrencyCode).HasColumnName("SubTotalCurrency").HasMaxLength(3);
            });

            builder.OwnsOne(o => o.FeeAmount, feeAmount =>
            {
                feeAmount.Property(m => m.Amount).HasColumnName("FeeAmount").HasPrecision(18, 2);
                feeAmount.Property(m => m.CurrencyCode).HasColumnName("FeeCurrency").HasMaxLength(3);
            });

            builder.OwnsOne(o => o.GrandTotal, grandTotal =>
            {
                grandTotal.Property(m => m.Amount).HasColumnName("GrandTotalAmount").HasPrecision(18, 2);
                grandTotal.Property(m => m.CurrencyCode).HasColumnName("GrandTotalCurrency").HasMaxLength(3);
            });
        });

        modelBuilder.Entity<OrderItem>(builder =>
        {
            builder.HasKey(oi => oi.Id);

            builder.OwnsOne(oi => oi.UnitPrice, unitPrice =>
            {
                unitPrice.Property(m => m.Amount).HasColumnName("UnitPriceAmount").HasPrecision(18, 2);
                unitPrice.Property(m => m.CurrencyCode).HasColumnName("UnitPriceCurrency").HasMaxLength(3);
            });

            builder.OwnsOne(oi => oi.TotalPrice, totalPrice =>
            {
                totalPrice.Property(m => m.Amount).HasColumnName("TotalPriceAmount").HasPrecision(18, 2);
                totalPrice.Property(m => m.CurrencyCode).HasColumnName("TotalPriceCurrency").HasMaxLength(3);
            });

            builder.HasOne(oi => oi.CreditPackage)
                .WithMany()
                .HasForeignKey(oi => oi.CreditPackageId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<OrderPayment>(builder =>
        {
            builder.HasKey(op => op.Id);
            
            builder.HasOne(op => op.PaymentChannel)
                .WithMany()
                .HasForeignKey(op => op.PaymentChannelId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<PaymentProvider>(builder =>
        {
            builder.HasKey(pp => pp.Id);
            
            builder.Navigation(pp => pp.Channels)
                .UsePropertyAccessMode(PropertyAccessMode.Field);
        });

        modelBuilder.Entity<PaymentChannel>(builder =>
        {
            builder.HasKey(pc => pc.Id);
            
            builder.HasOne<PaymentProvider>()
                .WithMany(pp => pp.Channels)
                .HasForeignKey(pc => pc.PaymentProviderId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<CreditPackage>(builder =>
        {
            builder.HasKey(cp => cp.Id);

            builder.OwnsOne(cp => cp.Price, price =>
            {
                price.Property(m => m.Amount).HasColumnName("PriceAmount").HasPrecision(18, 2);
                price.Property(m => m.CurrencyCode).HasColumnName("PriceCurrency").HasMaxLength(3);
            });

            builder.OwnsOne(cp => cp.DiscountPrice, discountPrice =>
            {
                discountPrice.Property(m => m.Amount).HasColumnName("DiscountAmount").HasPrecision(18, 2);
                discountPrice.Property(m => m.CurrencyCode).HasColumnName("DiscountCurrency").HasMaxLength(3);
            });
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
