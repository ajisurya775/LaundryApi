using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using LaundrySaas.Domain.Billing;
using LaundrySaas.Domain.Organization.Events;
using LaundrySaas.SharedKernel.Interfaces;

namespace LaundrySaas.Application.Billing.Events;

public class InitializeTenantBillingHandler : IDomainEventHandler<TenantRegisteredEvent>
{
    private readonly IPaymentMethodRepository _paymentMethodRepository;

    public InitializeTenantBillingHandler(
        IPaymentMethodRepository paymentMethodRepository)
    {
        _paymentMethodRepository = paymentMethodRepository;
    }

    public async Task HandleAsync(TenantRegisteredEvent domainEvent, CancellationToken cancellationToken = default)
    {
        // Seed Default Payment Methods
        var cashMethod = new PaymentMethod(Guid.NewGuid(), domainEvent.TenantId, "Cash", PaymentMethodType.Cash);
        var qrisMethod = new PaymentMethod(Guid.NewGuid(), domainEvent.TenantId, "QRIS", PaymentMethodType.QrCode);
        var tfMethod = new PaymentMethod(Guid.NewGuid(), domainEvent.TenantId, "Bank Transfer", PaymentMethodType.BankTransfer);

        await _paymentMethodRepository.AddRangeAsync(
            new List<PaymentMethod> { cashMethod, qrisMethod, tfMethod },
            cancellationToken);
    }
}
