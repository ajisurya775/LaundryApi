using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace LaundrySaas.Domain.Billing;

public interface IPaymentMethodRepository
{
    Task AddRangeAsync(IEnumerable<PaymentMethod> paymentMethods, CancellationToken cancellationToken = default);
}
