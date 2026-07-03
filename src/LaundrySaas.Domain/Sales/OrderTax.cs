using System;
using LaundrySaas.SharedKernel.Primitives;
using LaundrySaas.SharedKernel.ValueObjects;

namespace LaundrySaas.Domain.Sales;

public class OrderTax : Entity
{
    public Guid OrderId { get; private set; }
    public string Name { get; private set; }
    public TaxRate Rate { get; private set; }
    public Money Amount { get; private set; }

    public OrderTax(Guid id, Guid orderId, string name, TaxRate rate, Money amount) : base(id)
    {
        OrderId = orderId;
        Name = name;
        Rate = rate;
        Amount = amount;
    }

    private OrderTax()
    {
        Name = null!;
        Rate = null!;
        Amount = null!;
    }
}
