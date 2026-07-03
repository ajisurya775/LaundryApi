using System;
using LaundrySaas.SharedKernel.Primitives;
using LaundrySaas.SharedKernel.ValueObjects;

namespace LaundrySaas.Domain.Sales;

public class OrderItem : Entity
{
    public Guid OrderId { get; private set; }
    public string Name { get; private set; }
    public Money UnitPrice { get; private set; }
    public int Quantity { get; private set; }
    public Money TotalPrice { get; private set; }

    public OrderItem(
        Guid id,
        Guid orderId,
        string name,
        Money unitPrice,
        int quantity) : base(id)
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
        TotalPrice = unitPrice.Multiply(quantity);
    }

    private OrderItem()
    {
        Name = null!;
        UnitPrice = null!;
        TotalPrice = null!;
    }
}
