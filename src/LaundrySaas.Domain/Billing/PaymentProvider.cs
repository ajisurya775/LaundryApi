using LaundrySaas.SharedKernel.Primitives;

namespace LaundrySaas.Domain.Billing;

/// <summary>
/// Represents a payment gateway provider (e.g., Xendit, Midtrans, Doku).
/// Manageable entity — can be activated/deactivated from admin panel.
/// </summary>
public class PaymentProvider : Entity
{
    public string Name { get; private set; }
    public string Code { get; private set; }
    public string? Description { get; private set; }
    public bool IsActive { get; private set; }

    private readonly List<PaymentChannel> _channels = new();
    public IReadOnlyCollection<PaymentChannel> Channels => _channels.AsReadOnly();

    public PaymentProvider(Guid id, string name, string code, string? description = null) : base(id)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Provider name is required.", nameof(name));

        if (string.IsNullOrWhiteSpace(code))
            throw new ArgumentException("Provider code is required.", nameof(code));

        Name = name;
        Code = code.ToUpperInvariant();
        Description = description;
        IsActive = true;
    }

    // EF Core Constructor
    private PaymentProvider()
    {
        Name = null!;
        Code = null!;
    }

    public void UpdateDetails(string name, string? description)
    {
        Name = name;
        Description = description;
    }

    public void AddChannel(PaymentChannel channel)
    {
        if (_channels.Any(c => c.Code == channel.Code))
            throw new InvalidOperationException($"Channel with code '{channel.Code}' already exists for this provider.");

        _channels.Add(channel);
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
