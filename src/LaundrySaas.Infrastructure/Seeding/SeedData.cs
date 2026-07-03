using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using LaundrySaas.Domain.Identity;
using LaundrySaas.Domain.Organization;
using LaundrySaas.Domain.POS;
using LaundrySaas.Domain.Billing;
using LaundrySaas.Infrastructure.Persistence;

namespace LaundrySaas.Infrastructure.Seeding;

public static class SeedData
{
    public static async Task SeedAsync(ApplicationDbContext context)
    {
        var permissions = await SeedPermissions(context);
        var tenant = await SeedTenant(context);
        var branches = await SeedBranches(context, tenant);
        var roles = await SeedRoles(context, tenant, permissions);
        var users = await SeedUsers(context, tenant);
        var paymentMethods = await SeedPaymentMethods(context, tenant);
        var poses = await SeedPos(context, tenant, branches, paymentMethods);

        await SeedEmployeeAssignments(context, tenant, branches, roles, users, poses);
        await SeedSubscriptions(context, tenant);

        await SeedBilling.SeedAsync(context);

        await context.SaveChangesAsync();
    }

    private static async Task<List<Permission>> SeedPermissions(ApplicationDbContext context)
    {
        var existing = await context.Permissions.ToListAsync();
        if (existing.Any())
            return existing;

        var permissions = new List<Permission>
        {
            new(Guid.NewGuid(), "Dashboard.View",     "View Dashboard",        "SETTING",    "Access to view the dashboard"),
            new(Guid.NewGuid(), "Order.Create",        "Create Order",          "ORDER",      "Create new orders"),
            new(Guid.NewGuid(), "Order.Update",        "Update Order",          "ORDER",      "Update existing orders"),
            new(Guid.NewGuid(), "Order.Cancel",        "Cancel Order",          "ORDER",      "Cancel orders"),
            new(Guid.NewGuid(), "Customer.View",       "View Customers",        "CUSTOMER",   "View customer list"),
            new(Guid.NewGuid(), "Customer.Create",     "Create Customer",       "CUSTOMER",   "Create new customers"),
            new(Guid.NewGuid(), "Inventory.View",      "View Inventory",        "PRODUCT",    "View inventory items"),
            new(Guid.NewGuid(), "Inventory.Update",    "Update Inventory",      "PRODUCT",    "Update inventory items"),
            new(Guid.NewGuid(), "POS.OpenShift",       "Open POS Shift",        "POS",        "Open a POS shift"),
            new(Guid.NewGuid(), "POS.CloseShift",      "Close POS Shift",       "POS",        "Close a POS shift"),
            new(Guid.NewGuid(), "POS.Refund",          "POS Refund",            "POS",        "Process refunds at POS"),
            new(Guid.NewGuid(), "Employee.View",       "View Employees",        "EMPLOYEE",   "View employee list"),
            new(Guid.NewGuid(), "Employee.Create",     "Create Employee",       "EMPLOYEE",   "Create new employees"),
            new(Guid.NewGuid(), "Employee.Update",     "Update Employee",       "EMPLOYEE",   "Update employee details"),
            new(Guid.NewGuid(), "Report.View",         "View Reports",          "REPORT",     "View reports"),
            new(Guid.NewGuid(), "Billing.View",        "View Billing",          "REPORT",     "View billing information"),
            new(Guid.NewGuid(), "Billing.Manage",      "Manage Billing",        "SETTING",    "Manage billing settings and payments"),
            new(Guid.NewGuid(), "Tenant.Settings",     "Tenant Settings",       "SETTING",    "Manage tenant settings"),
        };

        context.Permissions.AddRange(permissions);
        await context.SaveChangesAsync();

        return permissions;
    }

    private static async Task<Tenant> SeedTenant(ApplicationDbContext context)
    {
        var tenant = await context.Tenants.IgnoreQueryFilters().FirstOrDefaultAsync(x => x.Name == "Demo Tenant");
        if (tenant != null)
            return tenant;

        tenant = new Tenant(
            Guid.NewGuid(),
            "Demo Tenant",
            "Demo Company",
            "ID",
            "+628123456789");

        context.Tenants.Add(tenant);
        await context.SaveChangesAsync();

        return tenant;
    }

    private static async Task<List<Branch>> SeedBranches(ApplicationDbContext context, Tenant tenant)
    {
        var branches = await context.Branches
            .IgnoreQueryFilters()
            .Where(x => x.TenantId == tenant.Id)
            .ToListAsync();

        if (branches.Any())
            return branches;

        branches = new List<Branch>
        {
            new(Guid.NewGuid(), tenant.Id, "Central Branch", "Jl. Merdeka No. 1", "+62 81234567890"),
            new(Guid.NewGuid(), tenant.Id, "West Branch", "Jl. Barat No. 22", "+62 82211112222")
        };

        context.Branches.AddRange(branches);
        await context.SaveChangesAsync();

        return branches;
    }

    private static async Task<Dictionary<string, Role>> SeedRoles(
        ApplicationDbContext context,
        Tenant tenant,
        List<Permission> permissions)
    {
        var existingRoles = await context.Roles
            .IgnoreQueryFilters()
            .Where(r => r.TenantId == tenant.Id)
            .ToListAsync();

        if (existingRoles.Any())
            return existingRoles.ToDictionary(r => r.Code, r => r);

        var permLookup = permissions.ToDictionary(p => p.Key, p => p.Id);

        var ownerRole = new Role(Guid.NewGuid(), tenant.Id, "Owner", "OWNER", 100, "Full system access", true);
        var managerRole = new Role(Guid.NewGuid(), tenant.Id, "Manager", "MANAGER", 80, "Operational management access", true);
        var cashierRole = new Role(Guid.NewGuid(), tenant.Id, "Cashier", "CASHIER", 10, "POS and order access", true);
        var accountantRole = new Role(Guid.NewGuid(), tenant.Id, "Accountant", "ACCOUNTANT", 50, "Billing and reporting access", true);

        foreach (var perm in permissions)
        {
            ownerRole.AddPermission(perm.Id);
        }

        var managerPerms = new[]
        {
            "Dashboard.View", "Order.Create", "Order.Update", "Order.Cancel",
            "Customer.View", "Customer.Create",
            "Inventory.View", "Inventory.Update",
            "POS.OpenShift", "POS.CloseShift", "POS.Refund",
            "Employee.View", "Employee.Create", "Employee.Update",
            "Report.View"
        };
        foreach (var key in managerPerms)
        {
            managerRole.AddPermission(permLookup[key]);
        }

        var cashierPerms = new[]
        {
            "Dashboard.View",
            "Order.Create", "Order.Update",
            "Customer.View", "Customer.Create",
            "POS.OpenShift", "POS.CloseShift", "POS.Refund"
        };
        foreach (var key in cashierPerms)
        {
            cashierRole.AddPermission(permLookup[key]);
        }

        var accountantPerms = new[]
        {
            "Dashboard.View",
            "Report.View",
            "Billing.View", "Billing.Manage"
        };
        foreach (var key in accountantPerms)
        {
            accountantRole.AddPermission(permLookup[key]);
        }

        context.Roles.AddRange(ownerRole, managerRole, cashierRole, accountantRole);
        await context.SaveChangesAsync();

        return new Dictionary<string, Role>
        {
            ["OWNER"] = ownerRole,
            ["MANAGER"] = managerRole,
            ["CASHIER"] = cashierRole,
            ["ACCOUNTANT"] = accountantRole
        };
    }

    private static async Task<Dictionary<string, User>> SeedUsers(ApplicationDbContext context, Tenant tenant)
    {
        var usersMap = new Dictionary<string, User>();

        var owner = await context.Users.IgnoreQueryFilters().FirstOrDefaultAsync(x => x.Email == "owner@example.com");
        if (owner == null)
        {
            owner = new User(
                Guid.NewGuid(),
                tenant.Id,
                "Owner",
                "owner@example.com",
                BCrypt.Net.BCrypt.HashPassword("Owner123!"));
            context.Users.Add(owner);
        }
        usersMap["OWNER"] = owner;

        var manager = await context.Users.IgnoreQueryFilters().FirstOrDefaultAsync(x => x.Email == "manager@example.com");
        if (manager == null)
        {
            manager = new User(
                Guid.NewGuid(),
                tenant.Id,
                "Manager",
                "manager@example.com",
                BCrypt.Net.BCrypt.HashPassword("Manager123!"));
            context.Users.Add(manager);
        }
        usersMap["MANAGER"] = manager;

        await context.SaveChangesAsync();
        return usersMap;
    }

    private static async Task<List<PaymentMethod>> SeedPaymentMethods(ApplicationDbContext context, Tenant tenant)
    {
        var paymentMethods = await context.PaymentMethods
            .IgnoreQueryFilters()
            .Where(x => x.TenantId == tenant.Id)
            .ToListAsync();

        if (paymentMethods.Any())
            return paymentMethods;

        paymentMethods = new List<PaymentMethod>
        {
            new(Guid.NewGuid(), tenant.Id, "Cash", PaymentMethodType.Cash),
            new(Guid.NewGuid(), tenant.Id, "QRIS", PaymentMethodType.QrCode),
            new(Guid.NewGuid(), tenant.Id, "Bank Transfer", PaymentMethodType.BankTransfer)
        };

        context.PaymentMethods.AddRange(paymentMethods);
        await context.SaveChangesAsync();

        return paymentMethods;
    }

    private static async Task<List<Pos>> SeedPos(
        ApplicationDbContext context,
        Tenant tenant,
        List<Branch> branches,
        List<PaymentMethod> paymentMethods)
    {
        var existingPos = await context.Poses
            .IgnoreQueryFilters()
            .Where(x => x.TenantId == tenant.Id)
            .ToListAsync();

        if (existingPos.Any())
            return existingPos;

        var allPos = new List<Pos>();

        foreach (var branch in branches)
        {
            var pos1 = new Pos(Guid.NewGuid(), tenant.Id, branch.Id, $"{branch.Name} - Kasir 1");
            var pos2 = new Pos(Guid.NewGuid(), tenant.Id, branch.Id, $"{branch.Name} - Kasir 2");

            foreach (var pm in paymentMethods)
            {
                pos1.AddPaymentMethod(pm.Id);
            }

            var cashPm = paymentMethods.FirstOrDefault(pm => pm.Name == "Cash");
            var qrisPm = paymentMethods.FirstOrDefault(pm => pm.Name == "QRIS");
            if (cashPm != null) pos2.AddPaymentMethod(cashPm.Id);
            if (qrisPm != null) pos2.AddPaymentMethod(qrisPm.Id);

            context.Poses.AddRange(pos1, pos2);
            allPos.AddRange(new[] { pos1, pos2 });
        }

        await context.SaveChangesAsync();
        return allPos;
    }

    private static async Task SeedEmployeeAssignments(
        ApplicationDbContext context,
        Tenant tenant,
        List<Branch> branches,
        Dictionary<string, Role> roles,
        Dictionary<string, User> users,
        List<Pos> poses)
    {
        var existingAssignments = await context.EmployeeAssignments
            .IgnoreQueryFilters()
            .AnyAsync(ua => ua.TenantId == tenant.Id);

        if (existingAssignments)
            return;

        var owner = users["OWNER"];
        var manager = users["MANAGER"];

        // Owner gets tenant-wide role assignment (branchId = null, posId = null)
        var ownerAssignment = new EmployeeAssignment(
            Guid.NewGuid(),
            tenant.Id,
            owner.Id,
            branchId: null,
            departmentId: null,
            posId: null,
            roleId: roles["OWNER"].Id,
            remarks: "System Owner Assignment");
        context.EmployeeAssignments.Add(ownerAssignment);

        // Manager gets assigned to each branch and POS as default
        foreach (var branch in branches)
        {
            var branchPoses = poses.Where(p => p.BranchId == branch.Id).ToList();
            var isFirstBranch = branch == branches.First();

            var assignment = new EmployeeAssignment(
                Guid.NewGuid(),
                tenant.Id,
                manager.Id,
                branchId: branch.Id,
                departmentId: null,
                posId: branchPoses.FirstOrDefault()?.Id,
                roleId: roles["MANAGER"].Id,
                isDefault: isFirstBranch,
                remarks: "Manager Branch Assignment");

            context.EmployeeAssignments.Add(assignment);
        }

        await context.SaveChangesAsync();
    }

    private static async Task SeedSubscriptions(ApplicationDbContext context, Tenant tenant)
    {
        var hasSubscription = await context.Subscriptions.AnyAsync(s => s.TenantId == tenant.Id);
        if (hasSubscription)
            return;

        // Give the Demo Tenant the Pro Plan (which was seeded in SeedBilling)
        var planId = Guid.Parse("22222222-2222-2222-2222-222222222222");

        var subscription = new Subscription(
            Guid.NewGuid(),
            tenant.Id,
            planId,
            DateTime.UtcNow,
            DateTime.UtcNow.AddMonths(1),
            "Active"
        );

        context.Subscriptions.Add(subscription);

        // Add some basic usage metrics for the tenant
        var usageMaxBranches = new SubscriptionUsage(
            Guid.NewGuid(),
            tenant.Id,
            subscription.Id,
            "MaxBranches",
            LaundrySaas.SharedKernel.ValueObjects.Quantity.From(2)
        );

        var usageMaxUsers = new SubscriptionUsage(
            Guid.NewGuid(),
            tenant.Id,
            subscription.Id,
            "MaxUsers",
            LaundrySaas.SharedKernel.ValueObjects.Quantity.From(3)
        );

        context.SubscriptionUsages.AddRange(usageMaxBranches, usageMaxUsers);
        await context.SaveChangesAsync();
    }
}