using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace LaundrySaas.Domain.Identity;

public interface IPermissionRepository
{
    Task<List<Permission>> GetAllAsync(CancellationToken cancellationToken = default);
}
