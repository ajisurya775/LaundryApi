using System;
using LaundrySaas.SharedKernel.Primitives;
using LaundrySaas.SharedKernel.ValueObjects;

namespace LaundrySaas.Domain.Sales;

public class OrderDiscount : Entity
{
    public Guid OrderId { get; private set; }
    public string Name { get; private set; }
    public Money Amount { get; private set; }

    public OrderDiscount(Guid id, Guid orderId, string name, Money amount) : base(id)
    {
        OrderId = orderId;
        Name = name;
        Amount = amount;
    }

    private OrderDiscount()
    {
        Name = null!;
        Amount = null!;
    }
}
