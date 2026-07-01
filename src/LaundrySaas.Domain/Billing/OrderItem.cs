using LaundrySaas.SharedKernel.Primitives;

namespace LaundrySaas.Domain.Billing;

/// <summary>
/// Detail item yang dibeli/dikenakan biaya di dalam suatu Order.
/// </summary>
public class OrderItem : Entity
{
    public Guid OrderId { get; private set; }
    public string Name { get; private set; }
    public Money UnitPrice { get; private set; }
    public int Quantity { get; private set; }
    public Money TotalPrice { get; private set; }

    /// <summary>
    /// Nilai kredit yang akan didapatkan/dipotong dari item ini (jika order bertipe TopUp/Refund).
    /// </summary>
    public decimal CreditAmount { get; private set; }

    // Hubungan opsional ke CreditPackage master data (karena tidak semua transaksi billing didasari pembelian paket master)
    public Guid? CreditPackageId { get; private set; }
    public CreditPackage? CreditPackage { get; private set; }

    public OrderItem(
        Guid id,
        Guid orderId,
        string name,
        Money unitPrice,
        int quantity,
        decimal creditAmount = 0,
        Guid? creditPackageId = null) : base(id)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Item name is required.", nameof(name));

        if (unitPrice == null)
            throw new ArgumentNullException(nameof(unitPrice));

        if (unitPrice.Amount < 0)
            throw new ArgumentException("Unit price cannot be negative.", nameof(unitPrice));

        if (quantity <= 0)
            throw new ArgumentException("Quantity must be greater than zero.", nameof(quantity));

        OrderId = orderId;
        Name = name;
        UnitPrice = unitPrice;
        Quantity = quantity;
        TotalPrice = new Money(unitPrice.Amount * quantity, unitPrice.CurrencyCode);
        CreditAmount = creditAmount * quantity;
        CreditPackageId = creditPackageId;
    }

    // EF Core Constructor
    private OrderItem()
    {
        Name = null!;
        UnitPrice = null!;
        TotalPrice = null!;
    }
}
