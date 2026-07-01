using LaundrySaas.SharedKernel.Primitives;

namespace LaundrySaas.Domain.Billing;

/// <summary>
/// Master data untuk paket kredit yang dapat dibeli oleh Tenant.
/// Dikelola secara global oleh system administrator.
/// </summary>
public class CreditPackage : Entity
{
    public string Name { get; private set; }
    public Money Price { get; private set; }
    public Money? DiscountPrice { get; private set; }
    public decimal CreditAmount { get; private set; }
    public string? Description { get; private set; }
    public bool IsActive { get; private set; }

    /// <summary>
    /// Harga aktif yang berlaku (menggunakan discountPrice jika tersedia, jika tidak menggunakan price).
    /// </summary>
    public Money ActivePrice => DiscountPrice ?? Price;

    public CreditPackage(
        Guid id,
        string name,
        Money price,
        decimal creditAmount,
        Money? discountPrice = null,
        string? description = null) : base(id)
    {
        ValidateInputs(name, price, creditAmount, discountPrice);

        Name = name;
        Price = price;
        CreditAmount = creditAmount;
        DiscountPrice = discountPrice;
        Description = description;
        IsActive = true;
    }

    // EF Core Constructor
    private CreditPackage()
    {
        Name = null!;
        Price = null!;
    }

    public void UpdateDetails(string name, Money price, decimal creditAmount, Money? discountPrice, string? description)
    {
        ValidateInputs(name, price, creditAmount, discountPrice);

        Name = name;
        Price = price;
        CreditAmount = creditAmount;
        DiscountPrice = discountPrice;
        Description = description;
    }

    public void Deactivate()
    {
        IsActive = false;
    }

    public void Activate()
    {
        IsActive = true;
    }

    private static void ValidateInputs(string name, Money price, decimal creditAmount, Money? discountPrice)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Package name is required.", nameof(name));

        if (price == null)
            throw new ArgumentNullException(nameof(price));

        if (price.Amount <= 0)
            throw new ArgumentException("Price amount must be greater than zero.", nameof(price));

        if (creditAmount <= 0)
            throw new ArgumentException("Credit amount must be greater than zero.", nameof(creditAmount));

        if (discountPrice != null)
        {
            if (discountPrice.Amount <= 0)
                throw new ArgumentException("Discount price amount must be greater than zero.", nameof(discountPrice));

            if (discountPrice.CurrencyCode != price.CurrencyCode)
                throw new ArgumentException("Discount price currency must match regular price currency.", nameof(discountPrice));

            if (discountPrice.Amount >= price.Amount)
                throw new ArgumentException("Discount price amount must be less than the regular price amount.", nameof(discountPrice));
        }
    }
}
