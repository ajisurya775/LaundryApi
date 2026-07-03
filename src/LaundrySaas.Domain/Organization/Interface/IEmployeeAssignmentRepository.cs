using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace LaundrySaas.Domain.Organization;

public interface IEmployeeAssignmentRepository
{
    Task AddAsync(EmployeeAssignment assignment, CancellationToken cancellationToken = default);
    Task<List<EmployeeAssignment>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
}
