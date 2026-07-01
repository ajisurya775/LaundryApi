using BCrypt.Net;
using LaundrySaas.Domain.MultiTenancy;
using LaundrySaas.Domain.Users;
using LaundrySaas.Domain.Billing;
using LaundrySaas.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace LaundrySaas.Infrastructure.Seeding;

public static class SeedData
{
    public static async Task SeedAsync(ApplicationDbContext context)
    {
        var tenant = await SeedTenant(context);
        var branches = await SeedBranches(context, tenant);
        var menus = await SeedMenus(context);
        await SeedUsers(context, tenant, branches);
        await SeedRoleMenuAccess(context, tenant, menus);

        var paymentMethods = await SeedPaymentMethods(context, tenant);
        await SeedPos(context, tenant, branches, paymentMethods);

        await SeedBilling.SeedAsync(context);

        await context.SaveChangesAsync();
    }

    private static async Task<Tenant> SeedTenant(ApplicationDbContext context)
    {
        var tenant = await context.Set<Tenant>()
            .FirstOrDefaultAsync(x => x.Name == "Demo Tenant");

        if (tenant != null)
            return tenant;

        tenant = new Tenant(
            Guid.NewGuid(),
            "Demo Tenant",
            "Demo Company");

        context.Set<Tenant>().Add(tenant);

        await context.SaveChangesAsync();

        return tenant;
    }

    private static async Task<List<Branch>> SeedBranches(
        ApplicationDbContext context,
        Tenant tenant)
    {
        var branches = await context.Set<Branch>()
            .IgnoreQueryFilters()
            .Where(x => x.TenantId == tenant.Id)
            .ToListAsync();

        if (branches.Any())
            return branches;

        branches =
        [
            new Branch(
                Guid.NewGuid(),
                tenant.Id,
                "Central Branch",
                "Jl. Merdeka No. 1",
                "+62 81234567890"),

            new Branch(
                Guid.NewGuid(),
                tenant.Id,
                "West Branch",
                "Jl. Barat No. 22",
                "+62 82211112222")
        ];

        context.Set<Branch>().AddRange(branches);

        await context.SaveChangesAsync();

        return branches;
    }

    private static async Task<List<MenuItem>> SeedMenus(ApplicationDbContext context)
    {
        var menus = await context.Set<MenuItem>()
            .OrderBy(x => x.OrderNumber)
            .ToListAsync();

        if (menus.Any())
            return menus;

        menus =
        [
            new MenuItem(Guid.NewGuid(),"dashboard","Dashboard","/dashboard","fa-solid fa-house",null,1),
            new MenuItem(Guid.NewGuid(),"orders","Orders","/orders","fa-solid fa-receipt",null,2),
            new MenuItem(Guid.NewGuid(),"customers","Customers","/customers","fa-solid fa-users",null,3),
            new MenuItem(Guid.NewGuid(),"settings","Settings","/settings","fa-solid fa-gear",null,4)
        ];

        context.Set<MenuItem>().AddRange(menus);

        await context.SaveChangesAsync();

        return menus;
    }

    private static async Task SeedUsers(
        ApplicationDbContext context,
        Tenant tenant,
        List<Branch> branches)
    {
        var ownerExists = await context.Set<User>()
            .IgnoreQueryFilters()
            .AnyAsync(x => x.Email == "owner@example.com");

        if (!ownerExists)
        {
            var owner = new User(
                Guid.NewGuid(),
                tenant.Id,
                "Owner",
                "owner@example.com",
                BCrypt.Net.BCrypt.HashPassword("Owner123!"),
                UserRole.Owner);

            context.Set<User>().Add(owner);
        }

        var managerExists = await context.Set<User>()
            .IgnoreQueryFilters()
            .AnyAsync(x => x.Email == "manager@example.com");

        if (!managerExists)
        {
            var manager = new User(
                Guid.NewGuid(),
                tenant.Id,
                "Manager",
                "manager@example.com",
                BCrypt.Net.BCrypt.HashPassword("Manager123!"),
                UserRole.Manager);

            foreach (var branch in branches)
            {
                manager.AssignToBranch(branch.Id);
            }

            context.Set<User>().Add(manager);
        }

        await context.SaveChangesAsync();
    }

    private static async Task SeedRoleMenuAccess(
        ApplicationDbContext context,
        Tenant tenant,
        List<MenuItem> menus)
    {
        if (await context.Set<RoleMenuAccess>().IgnoreQueryFilters().AnyAsync())
            return;

        var accesses = new List<RoleMenuAccess>();

        foreach (var menu in menus)
        {
            accesses.Add(new RoleMenuAccess(
                Guid.NewGuid(),
                tenant.Id,
                UserRole.Owner,
                menu.Id));
        }

        var dashboard = menus.First(x => x.Key == "dashboard");
        var orders = menus.First(x => x.Key == "orders");

        accesses.Add(new RoleMenuAccess(
            Guid.NewGuid(),
            tenant.Id,
            UserRole.Manager,
            dashboard.Id));

        accesses.Add(new RoleMenuAccess(
            Guid.NewGuid(),
            tenant.Id,
            UserRole.Manager,
            orders.Id));

        context.Set<RoleMenuAccess>().AddRange(accesses);

        await context.SaveChangesAsync();
    }

    private static async Task<List<PaymentMethod>> SeedPaymentMethods(
        ApplicationDbContext context,
        Tenant tenant)
    {
        var paymentMethods = await context.Set<PaymentMethod>()
            .IgnoreQueryFilters()
            .Where(x => x.TenantId == tenant.Id)
            .ToListAsync();

        if (paymentMethods.Any())
            return paymentMethods;

        paymentMethods =
        [
            new PaymentMethod(Guid.NewGuid(), tenant.Id, "Cash"),
            new PaymentMethod(Guid.NewGuid(), tenant.Id, "QRIS"),
            new PaymentMethod(Guid.NewGuid(), tenant.Id, "Bank Transfer")
        ];

        context.Set<PaymentMethod>().AddRange(paymentMethods);
        await context.SaveChangesAsync();

        return paymentMethods;
    }

    private static async Task SeedPos(
        ApplicationDbContext context,
        Tenant tenant,
        List<Branch> branches,
        List<PaymentMethod> paymentMethods)
    {
        var posExists = await context.Set<Pos>()
            .IgnoreQueryFilters()
            .AnyAsync(x => x.TenantId == tenant.Id);

        if (posExists)
            return;

        foreach (var branch in branches)
        {
            var pos1 = new Pos(Guid.NewGuid(), tenant.Id, branch.Id, $"{branch.Name} - Kasir 1");
            var pos2 = new Pos(Guid.NewGuid(), tenant.Id, branch.Id, $"{branch.Name} - Kasir 2");

            // Enable all payment methods for Kasir 1
            foreach (var pm in paymentMethods)
            {
                pos1.AddPaymentMethod(pm.Id);
            }

            // Enable only Cash & QRIS for Kasir 2
            var cashPm = paymentMethods.FirstOrDefault(pm => pm.Name == "Cash");
            var qrisPm = paymentMethods.FirstOrDefault(pm => pm.Name == "QRIS");
            if (cashPm != null) pos2.AddPaymentMethod(cashPm.Id);
            if (qrisPm != null) pos2.AddPaymentMethod(qrisPm.Id);

            context.Set<Pos>().AddRange(pos1, pos2);
        }

        await context.SaveChangesAsync();
    }
}