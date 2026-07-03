using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using LaundrySaas.Domain.Sales;
using LaundrySaas.Domain.POS;

namespace LaundrySaas.Infrastructure.Persistence.Configurations;

public class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.HasKey(o => o.Id);
        builder.Navigation(o => o.Items).UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.HasOne(o => o.Payment)
            .WithOne()
            .HasForeignKey<PaymentAllocation>(pa => pa.OrderId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne<Shift>()
            .WithMany()
            .HasForeignKey(o => o.ShiftId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired(false);

        builder.OwnsOne(o => o.SubTotal, subTotal =>
        {
            subTotal.Property(m => m.Amount).HasColumnName("SubTotalAmount").HasPrecision(18, 2);
            subTotal.Property(m => m.CurrencyCode).HasColumnName("SubTotalCurrency").HasMaxLength(3);
        });

        builder.OwnsOne(o => o.FeeAmount, feeAmount =>
        {
            feeAmount.Property(m => m.Amount).HasColumnName("FeeAmount").HasPrecision(18, 2);
            feeAmount.Property(m => m.CurrencyCode).HasColumnName("FeeCurrency").HasMaxLength(3);
        });

        builder.OwnsOne(o => o.GrandTotal, grandTotal =>
        {
            grandTotal.Property(m => m.Amount).HasColumnName("GrandTotalAmount").HasPrecision(18, 2);
            grandTotal.Property(m => m.CurrencyCode).HasColumnName("GrandTotalCurrency").HasMaxLength(3);
        });
    }
}

public class OrderItemConfiguration : IEntityTypeConfiguration<OrderItem>
{
    public void Configure(EntityTypeBuilder<OrderItem> builder)
    {
        builder.HasKey(oi => oi.Id);

        builder.OwnsOne(oi => oi.UnitPrice, unitPrice =>
        {
            unitPrice.Property(m => m.Amount).HasColumnName("UnitPriceAmount").HasPrecision(18, 2);
            unitPrice.Property(m => m.CurrencyCode).HasColumnName("UnitPriceCurrency").HasMaxLength(3);
        });

        builder.OwnsOne(oi => oi.TotalPrice, totalPrice =>
        {
            totalPrice.Property(m => m.Amount).HasColumnName("TotalPriceAmount").HasPrecision(18, 2);
            totalPrice.Property(m => m.CurrencyCode).HasColumnName("TotalPriceCurrency").HasMaxLength(3);
        });

        builder.HasOne<Order>()
            .WithMany(o => o.Items)
            .HasForeignKey(oi => oi.OrderId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class PaymentAllocationConfiguration : IEntityTypeConfiguration<PaymentAllocation>
{
    public void Configure(EntityTypeBuilder<PaymentAllocation> builder)
    {
        builder.HasKey(pa => pa.Id);
    }
}

public class PromotionConfiguration : IEntityTypeConfiguration<Promotion>
{
    public void Configure(EntityTypeBuilder<Promotion> builder)
    {
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Code).HasMaxLength(50);
    }
}

public class OrderDiscountConfiguration : IEntityTypeConfiguration<OrderDiscount>
{
    public void Configure(EntityTypeBuilder<OrderDiscount> builder)
    {
        builder.HasKey(od => od.Id);
        builder.OwnsOne(od => od.Amount, balance =>
        {
            balance.Property(m => m.Amount).HasColumnName("Amount").HasPrecision(18, 2);
            balance.Property(m => m.CurrencyCode).HasColumnName("CurrencyCode").HasMaxLength(3);
        });
    }
}

public class OrderTaxConfiguration : IEntityTypeConfiguration<OrderTax>
{
    public void Configure(EntityTypeBuilder<OrderTax> builder)
    {
        builder.HasKey(ot => ot.Id);
        builder.OwnsOne(ot => ot.Rate, rate =>
        {
            rate.Property(r => r.Rate).HasColumnName("TaxRate").HasPrecision(5, 2);
        });
        builder.OwnsOne(ot => ot.Amount, balance =>
        {
            balance.Property(m => m.Amount).HasColumnName("Amount").HasPrecision(18, 2);
            balance.Property(m => m.CurrencyCode).HasColumnName("CurrencyCode").HasMaxLength(3);
        });
    }
}

public class InvoiceConfiguration : IEntityTypeConfiguration<Invoice>
{
    public void Configure(EntityTypeBuilder<Invoice> builder)
    {
        builder.HasKey(i => i.Id);
        builder.Property(i => i.Number).HasMaxLength(50);
        builder.OwnsOne(i => i.Total, balance =>
        {
            balance.Property(m => m.Amount).HasColumnName("TotalAmount").HasPrecision(18, 2);
            balance.Property(m => m.CurrencyCode).HasColumnName("TotalCurrency").HasMaxLength(3);
        });
    }
}
