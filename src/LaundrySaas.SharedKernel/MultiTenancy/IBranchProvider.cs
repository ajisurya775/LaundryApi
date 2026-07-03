using System;

namespace LaundrySaas.SharedKernel.MultiTenancy;

public interface IBranchProvider
{
    Guid? BranchId { get; }
}
