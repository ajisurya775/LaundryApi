using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using LaundrySaas.Domain.Billing;

namespace LaundrySaas.Infrastructure.Persistence.Configurations;

public class PaymentMethodConfiguration : IEntityTypeConfiguration<PaymentMethod>
{
    public void Configure(EntityTypeBuilder<PaymentMethod> builder)
    {
        builder.HasKey(pm => pm.Id);
        builder.Property(pm => pm.Name).HasMaxLength(100);
        builder.Property(pm => pm.Type).HasConversion<string>().HasMaxLength(30);
        builder.Property(pm => pm.AssetUrl).HasMaxLength(500);
        builder.Property(pm => pm.MinTransaction).HasPrecision(18, 2);
        builder.Property(pm => pm.MaxTransaction).HasPrecision(18, 2);
    }
}

public class PaymentGatewayConfiguration : IEntityTypeConfiguration<PaymentGateway>
{
    public void Configure(EntityTypeBuilder<PaymentGateway> builder)
    {
        builder.HasKey(pg => pg.Id);
        builder.Property(pg => pg.Code).HasMaxLength(50);
    }
}

public class SaaSInvoiceConfiguration : IEntityTypeConfiguration<Invoice>
{
    public void Configure(EntityTypeBuilder<Invoice> builder)
    {
        builder.HasKey(i => i.Id);
        builder.Property(i => i.Number).HasMaxLength(50);
        builder.Property(i => i.CurrencyCode).HasMaxLength(3);
        builder.Property(i => i.Status).HasConversion<string>().HasMaxLength(20);

        builder.OwnsOne(i => i.Discount, m =>
        {
            m.Property(p => p.Amount).HasColumnName("DiscountAmount").HasPrecision(18, 2);
            m.Property(p => p.CurrencyCode).HasColumnName("DiscountCurrency").HasMaxLength(3);
        });

        builder.OwnsOne(i => i.Subtotal, m =>
        {
            m.Property(p => p.Amount).HasColumnName("SubtotalAmount").HasPrecision(18, 2);
            m.Property(p => p.CurrencyCode).HasColumnName("SubtotalCurrency").HasMaxLength(3);
        });

        builder.OwnsOne(i => i.GrandTotal, m =>
        {
            m.Property(p => p.Amount).HasColumnName("GrandTotalAmount").HasPrecision(18, 2);
            m.Property(p => p.CurrencyCode).HasColumnName("GrandTotalCurrency").HasMaxLength(3);
        });

        builder.OwnsOne(i => i.PriceDetails, pd =>
        {
            pd.OwnsOne(p => p.Subtotal, m =>
            {
                m.Property(x => x.Amount).HasColumnName("PD_SubtotalAmount").HasPrecision(18, 2);
                m.Property(x => x.CurrencyCode).HasColumnName("PD_SubtotalCurrency").HasMaxLength(3);
            });

            pd.OwnsOne(p => p.DiscountAmount, m =>
            {
                m.Property(x => x.Amount).HasColumnName("PD_DiscountAmount").HasPrecision(18, 2);
                m.Property(x => x.CurrencyCode).HasColumnName("PD_DiscountCurrency").HasMaxLength(3);
            });

            pd.Property(p => p.TaxName).HasColumnName("PD_TaxName").HasMaxLength(50);

            pd.OwnsOne(p => p.TaxRate, tr =>
            {
                tr.Property(x => x.Rate).HasColumnName("PD_TaxRate").HasPrecision(5, 2);
            });

            pd.OwnsOne(p => p.TaxAmount, m =>
            {
                m.Property(x => x.Amount).HasColumnName("PD_TaxAmount").HasPrecision(18, 2);
                m.Property(x => x.CurrencyCode).HasColumnName("PD_TaxCurrency").HasMaxLength(3);
            });

            pd.OwnsOne(p => p.GrandTotal, m =>
            {
                m.Property(x => x.Amount).HasColumnName("PD_GrandTotalAmount").HasPrecision(18, 2);
                m.Property(x => x.CurrencyCode).HasColumnName("PD_GrandTotalCurrency").HasMaxLength(3);
            });

            pd.OwnsOne(p => p.TotalPaid, m =>
            {
                m.Property(x => x.Amount).HasColumnName("PD_TotalPaidAmount").HasPrecision(18, 2);
                m.Property(x => x.CurrencyCode).HasColumnName("PD_TotalPaidCurrency").HasMaxLength(3);
            });

            pd.OwnsOne(p => p.TotalDue, m =>
            {
                m.Property(x => x.Amount).HasColumnName("PD_TotalDueAmount").HasPrecision(18, 2);
                m.Property(x => x.CurrencyCode).HasColumnName("PD_TotalDueCurrency").HasMaxLength(3);
            });
        });

        builder.HasMany(i => i.Items)
            .WithOne()
            .HasForeignKey(item => item.InvoiceId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(i => i.Payments)
            .WithOne()
            .HasForeignKey(p => p.InvoiceId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(i => i.Items).UsePropertyAccessMode(PropertyAccessMode.Field);
        builder.Navigation(i => i.Payments).UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}

public class InvoiceItemConfiguration : IEntityTypeConfiguration<InvoiceItem>
{
    public void Configure(EntityTypeBuilder<InvoiceItem> builder)
    {
        builder.HasKey(i => i.Id);
        builder.Property(i => i.Name).HasMaxLength(200);
        builder.Property(i => i.Description).HasMaxLength(500);

        builder.OwnsOne(i => i.UnitPrice, m =>
        {
            m.Property(p => p.Amount).HasColumnName("UnitPriceAmount").HasPrecision(18, 2);
            m.Property(p => p.CurrencyCode).HasColumnName("UnitPriceCurrency").HasMaxLength(3);
        });

        builder.OwnsOne(i => i.TotalPrice, m =>
        {
            m.Property(p => p.Amount).HasColumnName("TotalPriceAmount").HasPrecision(18, 2);
            m.Property(p => p.CurrencyCode).HasColumnName("TotalPriceCurrency").HasMaxLength(3);
        });
    }
}

public class InvoicePaymentConfiguration : IEntityTypeConfiguration<InvoicePayment>
{
    public void Configure(EntityTypeBuilder<InvoicePayment> builder)
    {
        builder.HasKey(p => p.Id);
        builder.Property(p => p.ReferenceNumber).HasMaxLength(100);
        builder.Property(p => p.Notes).HasMaxLength(500);

        builder.OwnsOne(p => p.Amount, m =>
        {
            m.Property(x => x.Amount).HasColumnName("PaymentAmount").HasPrecision(18, 2);
            m.Property(x => x.CurrencyCode).HasColumnName("PaymentCurrency").HasMaxLength(3);
        });
    }
}

public class SubscriptionConfiguration : IEntityTypeConfiguration<Subscription>
{
    public void Configure(EntityTypeBuilder<Subscription> builder)
    {
        builder.HasKey(s => s.Id);
        builder.Property(s => s.Status).HasMaxLength(20);
    }
}

public class SubscriptionPlanConfiguration : IEntityTypeConfiguration<SubscriptionPlan>
{
    public void Configure(EntityTypeBuilder<SubscriptionPlan> builder)
    {
        builder.HasKey(sp => sp.Id);
        builder.Property(sp => sp.Name).HasMaxLength(100);
        builder.Property(sp => sp.ExtraCredit).HasPrecision(18, 2);
        
        builder.OwnsOne(sp => sp.Price, price =>
        {
            price.Property(m => m.Amount).HasColumnName("PriceAmount").HasPrecision(18, 2);
            price.Property(m => m.CurrencyCode).HasColumnName("PriceCurrency").HasMaxLength(3);
        });
    }
}

public class SubscriptionUsageConfiguration : IEntityTypeConfiguration<SubscriptionUsage>
{
    public void Configure(EntityTypeBuilder<SubscriptionUsage> builder)
    {
        builder.HasKey(su => su.Id);
        builder.Property(su => su.FeatureName).HasMaxLength(50);
        builder.OwnsOne(su => su.UsedAmount, usage =>
        {
            usage.Property(u => u.Value).HasColumnName("UsedAmount").HasPrecision(18, 3);
        });
    }
}
