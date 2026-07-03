using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using LaundrySaas.Domain.Organization;

namespace LaundrySaas.Infrastructure.Persistence.Repositories;

public class EmployeeAssignmentRepository : IEmployeeAssignmentRepository
{
    private readonly ApplicationDbContext _db;

    public EmployeeAssignmentRepository(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task AddAsync(EmployeeAssignment assignment, CancellationToken cancellationToken = default)
    {
        await _db.EmployeeAssignments.AddAsync(assignment, cancellationToken);
    }

    public async Task<List<EmployeeAssignment>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _db.EmployeeAssignments.IgnoreQueryFilters().Where(ua => ua.UserId == userId).ToListAsync(cancellationToken);
    }
}
