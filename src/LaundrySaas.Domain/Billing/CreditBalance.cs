using LaundrySaas.Domain.Billing.Events;
using LaundrySaas.Domain.Billing.Exceptions;
using LaundrySaas.SharedKernel.Primitives;

namespace LaundrySaas.Domain.Billing;

/// <summary>
/// Aggregate Root representing a tenant's credit balance.
/// Each tenant has exactly one CreditBalance.
/// All balance mutations go through this aggregate to enforce invariants.
/// </summary>
public class CreditBalance : AggregateRoot
{
    private readonly List<Order> _orders = new();

    public Guid TenantId { get; private set; }
    public decimal Balance { get; private set; }
    public string CurrencyCode { get; private set; }
    public decimal LowBalanceThreshold { get; private set; }
    public bool IsActive { get; private set; }

    public IReadOnlyCollection<Order> Orders => _orders.AsReadOnly();

    public CreditBalance(Guid id, Guid tenantId, string currencyCode = "IDR", decimal lowBalanceThreshold = 0) : base(id)
    {
        TenantId = tenantId;
        Balance = 0;
        CurrencyCode = currencyCode.ToUpperInvariant();
        LowBalanceThreshold = lowBalanceThreshold;
        IsActive = true;
    }

    // EF Core Constructor
    private CreditBalance()
    {
        CurrencyCode = null!;
    }

    // ========================================================
    // Direct operations (balance changes immediately)
    // ========================================================

    /// <summary>
    /// Adds credit directly (manual top-up by admin). Balance changes immediately.
    /// </summary>
    public void TopUp(Money amount, string referenceNumber, string description, Guid? performedBy = null)
    {
        if (amount == null)
            throw new ArgumentNullException(nameof(amount));

        if (amount.Amount <= 0)
            throw new ArgumentException("Top-up amount must be greater than zero.", nameof(amount));

        if (amount.CurrencyCode != CurrencyCode)
            throw new InvalidOperationException($"Cannot top-up using currency '{amount.CurrencyCode}' on a credit balance in '{CurrencyCode}'.");

        if (string.IsNullOrWhiteSpace(referenceNumber))
            throw new ArgumentException("Reference number is required.", nameof(referenceNumber));

        Balance += amount.Amount;

        var order = new Order(
            id: Guid.NewGuid(),
            tenantId: TenantId,
            creditBalanceId: Id,
            type: OrderType.TopUp,
            referenceNumber: referenceNumber,
            description: description ?? string.Empty,
            currencyCode: CurrencyCode,
            performedBy: performedBy);

        order.AddItem("Manual Top-Up", amount, 1, creditAmount: amount.Amount);
        order.MarkAsSucceeded(DateTime.UtcNow);

        _orders.Add(order);

        RaiseDomainEvent(new CreditToppedUpEvent(TenantId, amount.Amount, Balance, referenceNumber));
    }

    /// <summary>
    /// Deducts credit from the balance. Throws if insufficient.
    /// </summary>
    public void Deduct(Money amount, OrderType type, string referenceNumber, string description, Guid? performedBy = null)
    {
        if (amount == null)
            throw new ArgumentNullException(nameof(amount));

        if (amount.Amount <= 0)
            throw new ArgumentException("Deduction amount must be greater than zero.", nameof(amount));

        if (amount.CurrencyCode != CurrencyCode)
            throw new InvalidOperationException($"Cannot deduct using currency '{amount.CurrencyCode}' on a credit balance in '{CurrencyCode}'.");

        if (string.IsNullOrWhiteSpace(referenceNumber))
            throw new ArgumentException("Reference number is required.", nameof(referenceNumber));

        if (type == OrderType.TopUp)
            throw new ArgumentException("Cannot use TopUp type for deduction. Use Refund or Adjustment instead.", nameof(type));

        if (!HasSufficientBalance(amount.Amount))
            throw new InsufficientCreditException(TenantId, Balance, amount.Amount);

        Balance -= amount.Amount;

        var order = new Order(
            id: Guid.NewGuid(),
            tenantId: TenantId,
            creditBalanceId: Id,
            type: type,
            referenceNumber: referenceNumber,
            description: description ?? string.Empty,
            currencyCode: CurrencyCode,
            performedBy: performedBy);

        order.AddItem("Usage Deduction", amount, 1, creditAmount: -amount.Amount);
        order.MarkAsSucceeded(DateTime.UtcNow);

        _orders.Add(order);

        RaiseDomainEvent(new CreditDeductedEvent(TenantId, amount.Amount, Balance, type, referenceNumber));

        if (LowBalanceThreshold > 0 && Balance <= LowBalanceThreshold)
        {
            RaiseDomainEvent(new LowCreditBalanceEvent(TenantId, Balance, LowBalanceThreshold));
        }
    }

    /// <summary>
    /// Refunds credit back to the balance. Balance changes immediately.
    /// </summary>
    public void Refund(Money amount, string referenceNumber, string description, Guid? performedBy = null)
    {
        if (amount == null)
            throw new ArgumentNullException(nameof(amount));

        if (amount.Amount <= 0)
            throw new ArgumentException("Refund amount must be greater than zero.", nameof(amount));

        if (amount.CurrencyCode != CurrencyCode)
            throw new InvalidOperationException($"Cannot refund using currency '{amount.CurrencyCode}' on a credit balance in '{CurrencyCode}'.");

        if (string.IsNullOrWhiteSpace(referenceNumber))
            throw new ArgumentException("Reference number is required.", nameof(referenceNumber));

        Balance += amount.Amount;

        var order = new Order(
            id: Guid.NewGuid(),
            tenantId: TenantId,
            creditBalanceId: Id,
            type: OrderType.Refund,
            referenceNumber: referenceNumber,
            description: description ?? string.Empty,
            currencyCode: CurrencyCode,
            performedBy: performedBy);

        order.AddItem("Refund", amount, 1, creditAmount: amount.Amount);
        order.MarkAsSucceeded(DateTime.UtcNow);

        _orders.Add(order);

        RaiseDomainEvent(new CreditToppedUpEvent(TenantId, amount.Amount, Balance, referenceNumber));
    }

    // ========================================================
    // Gateway operations (balance changes after confirmation)
    // ========================================================

    /// <summary>
    /// Initiates a top-up via payment gateway. Creates a Pending order.
    /// Balance does NOT change yet — call ConfirmTopUp() when gateway reports success.
    /// Returns the created order.
    /// </summary>
    public Order InitiateTopUp(
        Money amount,
        PaymentChannel paymentChannel,
        string description,
        Guid? performedBy = null)
    {
        if (amount == null)
            throw new ArgumentNullException(nameof(amount));

        if (amount.Amount <= 0)
            throw new ArgumentException("Top-up amount must be greater than zero.", nameof(amount));

        if (amount.CurrencyCode != CurrencyCode)
            throw new InvalidOperationException($"Cannot purchase top-up in currency '{amount.CurrencyCode}' for a balance in '{CurrencyCode}'.");

        if (paymentChannel == null)
            throw new ArgumentNullException(nameof(paymentChannel));

        if (!paymentChannel.IsAmountAllowed(amount))
            throw new ArgumentException($"Amount {amount} is not allowed for payment channel '{paymentChannel.Name}'.", nameof(amount));

        var orderId = Guid.NewGuid();
        var order = new Order(
            id: orderId,
            tenantId: TenantId,
            creditBalanceId: Id,
            type: OrderType.TopUp,
            referenceNumber: $"TOPUP-{orderId:N}",
            description: description ?? string.Empty,
            currencyCode: CurrencyCode,
            performedBy: performedBy);

        order.AddItem("Custom Credit Top-Up", amount, 1, creditAmount: amount.Amount);
        order.SetupGatewayPayment(paymentChannel);

        _orders.Add(order);

        return order;
    }

    /// <summary>
    /// Initiates a top-up via payment gateway using a pre-defined Credit Package.
    /// </summary>
    public Order InitiateTopUp(
        CreditPackage creditPackage,
        PaymentChannel paymentChannel,
        string description,
        Guid? performedBy = null)
    {
        if (creditPackage == null)
            throw new ArgumentNullException(nameof(creditPackage));

        if (paymentChannel == null)
            throw new ArgumentNullException(nameof(paymentChannel));

        if (!creditPackage.IsActive)
            throw new InvalidOperationException($"Credit package '{creditPackage.Name}' is not active.");

        // Active price is what the tenant pays (taking into account any discounts)
        var priceToPay = creditPackage.ActivePrice;

        if (priceToPay.CurrencyCode != CurrencyCode)
            throw new InvalidOperationException($"Credit package price currency '{priceToPay.CurrencyCode}' does not match tenant balance currency '{CurrencyCode}'.");

        if (!paymentChannel.IsAmountAllowed(priceToPay))
            throw new ArgumentException($"Package price {priceToPay} is not allowed for payment channel '{paymentChannel.Name}'.", nameof(paymentChannel));

        var orderId = Guid.NewGuid();
        var order = new Order(
            id: orderId,
            tenantId: TenantId,
            creditBalanceId: Id,
            type: OrderType.TopUp,
            referenceNumber: $"TOPUP-{orderId:N}",
            description: description ?? string.Empty,
            currencyCode: CurrencyCode,
            performedBy: performedBy);

        // Link the purchased package to the order item
        order.AddItem(
            name: creditPackage.Name,
            unitPrice: priceToPay,
            quantity: 1,
            creditAmount: creditPackage.CreditAmount,
            creditPackageId: creditPackage.Id);

        order.SetupGatewayPayment(paymentChannel);

        _orders.Add(order);

        return order;
    }

    /// <summary>
    /// Confirms a pending gateway top-up. Updates balance and marks order as Succeeded.
    /// Called when receiving a success webhook from the payment gateway.
    /// </summary>
    public void ConfirmTopUp(Guid orderId, DateTime paidAt)
    {
        var order = _orders.FirstOrDefault(o => o.Id == orderId)
            ?? throw new InvalidOperationException($"Order '{orderId}' not found in this CreditBalance.");

        if (order.Status != OrderStatus.Pending && order.Status != OrderStatus.Processing)
            throw new InvalidOperationException($"Order '{orderId}' is not in a pending or processing state. Current status: '{order.Status}'.");

        var creditAmount = order.GetTotalCreditAmount();
        Balance += creditAmount;

        order.MarkAsSucceeded(paidAt);

        RaiseDomainEvent(new CreditToppedUpEvent(
            TenantId,
            creditAmount,
            Balance,
            order.Payment?.GatewayPaymentId ?? order.Payment?.GatewayReferenceId ?? order.ReferenceNumber));
    }

    /// <summary>
    /// Marks a pending gateway top-up as failed.
    /// </summary>
    public void FailTopUp(Guid orderId, string reason)
    {
        var order = _orders.FirstOrDefault(o => o.Id == orderId)
            ?? throw new InvalidOperationException($"Order '{orderId}' not found in this CreditBalance.");

        order.MarkAsFailed(reason);
    }

    /// <summary>
    /// Marks a pending gateway top-up as expired.
    /// </summary>
    public void ExpireTopUp(Guid orderId)
    {
        var order = _orders.FirstOrDefault(o => o.Id == orderId)
            ?? throw new InvalidOperationException($"Order '{orderId}' not found in this CreditBalance.");

        order.MarkAsExpired();
    }

    // ========================================================
    // Query methods
    // ========================================================

    public bool HasSufficientBalance(decimal amount) => Balance >= amount;

    public void SetLowBalanceThreshold(decimal threshold)
    {
        if (threshold < 0)
            throw new ArgumentException("Threshold cannot be negative.", nameof(threshold));

        LowBalanceThreshold = threshold;
    }

    public void Deactivate()
    {
        IsActive = false;
    }

    public void Activate()
    {
        IsActive = true;
    }
}
