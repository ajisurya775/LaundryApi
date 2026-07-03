using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LaundrySaas.Domain.Identity;
using LaundrySaas.Domain.Organization;
using LaundrySaas.Domain.Organization.Events;
using LaundrySaas.SharedKernel.Interfaces;

namespace LaundrySaas.Application.Organization.Events;

public class InitializeTenantOrganizationHandler : IDomainEventHandler<TenantRegisteredEvent>
{
    private readonly IBranchRepository _branchRepository;
    private readonly IRoleRepository _roleRepository;
    private readonly IPermissionRepository _permissionRepository;
    private readonly IEmployeeAssignmentRepository _employeeAssignmentRepository;

    public InitializeTenantOrganizationHandler(
        IBranchRepository branchRepository,
        IRoleRepository roleRepository,
        IPermissionRepository permissionRepository,
        IEmployeeAssignmentRepository employeeAssignmentRepository)
    {
        _branchRepository = branchRepository;
        _roleRepository = roleRepository;
        _permissionRepository = permissionRepository;
        _employeeAssignmentRepository = employeeAssignmentRepository;
    }

    public async Task HandleAsync(TenantRegisteredEvent domainEvent, CancellationToken cancellationToken = default)
    {
        // 1. Create Default Branch
        var branch = new Branch(domainEvent.DefaultBranchId, domainEvent.TenantId, "Central Branch", "Default Address", "-");
        await _branchRepository.AddAsync(branch, cancellationToken);

        // 2. Create System Roles
        var permissions = await _permissionRepository.GetAllAsync(cancellationToken);
        
        var ownerRole = new Role(Guid.NewGuid(), domainEvent.TenantId, "Owner", "OWNER", 100, "Full system access", true);
        var managerRole = new Role(Guid.NewGuid(), domainEvent.TenantId, "Manager", "MANAGER", 80, "Operational management access", true);
        var cashierRole = new Role(Guid.NewGuid(), domainEvent.TenantId, "Cashier", "CASHIER", 10, "POS and order access", true);
        var accountantRole = new Role(Guid.NewGuid(), domainEvent.TenantId, "Accountant", "ACCOUNTANT", 50, "Billing and reporting access", true);

        // Assign permissions to Owner
        foreach (var perm in permissions)
        {
            ownerRole.AddPermission(perm.Id);
        }

        // Assign permissions to Manager
        var managerPermKeys = new HashSet<string>
        {
            "Dashboard.View", "Order.Create", "Order.Update", "Order.Cancel",
            "Customer.View", "Customer.Create",
            "Inventory.View", "Inventory.Update",
            "POS.OpenShift", "POS.CloseShift", "POS.Refund",
            "Employee.View", "Employee.Create", "Employee.Update",
            "Report.View"
        };
        foreach (var perm in permissions.Where(p => managerPermKeys.Contains(p.Key)))
        {
            managerRole.AddPermission(perm.Id);
        }

        // Assign permissions to Cashier
        var cashierPermKeys = new HashSet<string>
        {
            "Dashboard.View",
            "Order.Create", "Order.Update",
            "Customer.View", "Customer.Create",
            "POS.OpenShift", "POS.CloseShift", "POS.Refund"
        };
        foreach (var perm in permissions.Where(p => cashierPermKeys.Contains(p.Key)))
        {
            cashierRole.AddPermission(perm.Id);
        }

        // Assign permissions to Accountant
        var accountantPermKeys = new HashSet<string>
        {
            "Dashboard.View",
            "Report.View",
            "Billing.View", "Billing.Manage"
        };
        foreach (var perm in permissions.Where(p => accountantPermKeys.Contains(p.Key)))
        {
            accountantRole.AddPermission(perm.Id);
        }

        _roleRepository.AddRange(new List<Role> { ownerRole, managerRole, cashierRole, accountantRole });

        // 3. Assign Owner role to Default Branch and POS
        var assignment = new EmployeeAssignment(
            id: Guid.NewGuid(),
            tenantId: domainEvent.TenantId,
            userId: domainEvent.OwnerId,
            branchId: domainEvent.DefaultBranchId,
            departmentId: null,
            posId: domainEvent.DefaultPosId,
            roleId: ownerRole.Id,
            employmentType: "Permanent",
            status: "Active",
            effectiveFrom: DateTime.UtcNow,
            effectiveUntil: null,
            isDefault: true,
            priority: 100,
            remarks: "System Owner Assignment with POS");

        await _employeeAssignmentRepository.AddAsync(assignment, cancellationToken);
    }
}
