using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using LaundrySaas.Domain.POS;
using LaundrySaas.Domain.Organization;

namespace LaundrySaas.Infrastructure.Persistence.Configurations;

public class PosConfiguration : IEntityTypeConfiguration<Pos>
{
    public void Configure(EntityTypeBuilder<Pos> builder)
    {
        builder.HasKey(p => p.Id);
        builder.Navigation(p => p.PaymentMethods).UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.HasOne<Branch>()
            .WithMany()
            .HasForeignKey(p => p.BranchId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class PosPaymentMethodConfiguration : IEntityTypeConfiguration<PosPaymentMethod>
{
    public void Configure(EntityTypeBuilder<PosPaymentMethod> builder)
    {
        builder.HasKey(ppm => ppm.Id);
        builder.HasOne<Pos>()
            .WithMany(p => p.PaymentMethods)
            .HasForeignKey(ppm => ppm.PosId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class ShiftConfiguration : IEntityTypeConfiguration<Shift>
{
    public void Configure(EntityTypeBuilder<Shift> builder)
    {
        builder.HasKey(s => s.Id);
        builder.Property(s => s.Status).HasMaxLength(20);

        builder.OwnsOne(s => s.OpeningBalance, balance =>
        {
            balance.Property(m => m.Amount).HasColumnName("OpeningBalance").HasPrecision(18, 2);
            balance.Property(m => m.CurrencyCode).HasColumnName("OpeningCurrency").HasMaxLength(3);
        });

        builder.OwnsOne(s => s.ClosingBalance, balance =>
        {
            balance.Property(m => m.Amount).HasColumnName("ClosingBalance").HasPrecision(18, 2);
            balance.Property(m => m.CurrencyCode).HasColumnName("ClosingCurrency").HasMaxLength(3);
        });

        builder.HasOne<Pos>()
            .WithMany()
            .HasForeignKey(s => s.PosId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class CashDrawerConfiguration : IEntityTypeConfiguration<CashDrawer>
{
    public void Configure(EntityTypeBuilder<CashDrawer> builder)
    {
        builder.HasKey(cd => cd.Id);

        builder.OwnsOne(cd => cd.Balance, balance =>
        {
            balance.Property(m => m.Amount).HasColumnName("BalanceAmount").HasPrecision(18, 2);
            balance.Property(m => m.CurrencyCode).HasColumnName("BalanceCurrency").HasMaxLength(3);
        });
    }
}

public class CashMovementConfiguration : IEntityTypeConfiguration<CashMovement>
{
    public void Configure(EntityTypeBuilder<CashMovement> builder)
    {
        builder.HasKey(cm => cm.Id);

        builder.OwnsOne(cm => cm.Amount, balance =>
        {
            balance.Property(m => m.Amount).HasColumnName("Amount").HasPrecision(18, 2);
            balance.Property(m => m.CurrencyCode).HasColumnName("Currency").HasMaxLength(3);
        });
    }
}
