using System;
using System.Threading;
using System.Threading.Tasks;
using LaundrySaas.Domain.Organization.Events;
using LaundrySaas.Domain.POS;
using LaundrySaas.SharedKernel.Interfaces;

namespace LaundrySaas.Application.POS.Events;

public class InitializeTenantPosHandler : IDomainEventHandler<TenantRegisteredEvent>
{
    private readonly IPosRepository _posRepository;

    public InitializeTenantPosHandler(IPosRepository posRepository)
    {
        _posRepository = posRepository;
    }

    public async Task HandleAsync(TenantRegisteredEvent domainEvent, CancellationToken cancellationToken = default)
    {
        // Seed default POS entity
        var pos = new Pos(domainEvent.DefaultPosId, domainEvent.TenantId, domainEvent.DefaultBranchId, "Kasir 1");
        
        // Add default cash/qris/transfer methods
        pos.AddPaymentMethod(Guid.NewGuid()); // Cash
        pos.AddPaymentMethod(Guid.NewGuid()); // QRIS
        pos.AddPaymentMethod(Guid.NewGuid()); // Bank Transfer

        await _posRepository.AddAsync(pos, cancellationToken);
    }
}
