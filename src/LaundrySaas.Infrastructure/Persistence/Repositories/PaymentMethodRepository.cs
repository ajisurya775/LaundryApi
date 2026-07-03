using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using LaundrySaas.Domain.Billing;

namespace LaundrySaas.Infrastructure.Persistence.Repositories;

public class PaymentMethodRepository : IPaymentMethodRepository
{
    private readonly ApplicationDbContext _db;

    public PaymentMethodRepository(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task AddRangeAsync(IEnumerable<PaymentMethod> paymentMethods, CancellationToken cancellationToken = default)
    {
        await _db.PaymentMethods.AddRangeAsync(paymentMethods, cancellationToken);
    }
}
