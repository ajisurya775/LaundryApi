using LaundrySaas.Domain.Billing;
using LaundrySaas.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace LaundrySaas.Infrastructure.Seeding;

public static class SeedBilling
{
    public static async Task SeedAsync(ApplicationDbContext context)
    {
        var xenditProvider = await SeedPaymentProvider(context);
        await SeedPaymentChannels(context, xenditProvider);
        await SeedCreditPackages(context);
    }

    private static async Task<PaymentProvider> SeedPaymentProvider(ApplicationDbContext context)
    {
        var xenditProvider = await context.Set<PaymentProvider>()
            .FirstOrDefaultAsync(p => p.Code == "XENDIT");

        if (xenditProvider == null)
        {
            xenditProvider = new PaymentProvider(
                Guid.NewGuid(),
                "Xendit Payment Gateway",
                "XENDIT",
                "Integrasi Xendit untuk pembayaran otomatis via Virtual Account, eWallet, dan QRIS");

            context.Set<PaymentProvider>().Add(xenditProvider);
            await context.SaveChangesAsync();
        }

        return xenditProvider;
    }

    private static async Task SeedPaymentChannels(ApplicationDbContext context, PaymentProvider provider)
    {
        var hasChannels = await context.Set<PaymentChannel>()
            .AnyAsync(c => c.PaymentProviderId == provider.Id);

        if (hasChannels)
            return;

        var channels = new List<PaymentChannel>
        {
            // Virtual Accounts
            new PaymentChannel(Guid.NewGuid(), provider.Id, "BCA Virtual Account", "BCA", PaymentMethodType.VirtualAccount, null, 10000, 50000000, 4500, 0),
            new PaymentChannel(Guid.NewGuid(), provider.Id, "BNI Virtual Account", "BNI", PaymentMethodType.VirtualAccount, null, 10000, 50000000, 4500, 0),
            new PaymentChannel(Guid.NewGuid(), provider.Id, "BRI Virtual Account", "BRI", PaymentMethodType.VirtualAccount, null, 10000, 50000000, 4500, 0),
            new PaymentChannel(Guid.NewGuid(), provider.Id, "Mandiri Virtual Account", "MANDIRI", PaymentMethodType.VirtualAccount, null, 10000, 50000000, 4500, 0),
            new PaymentChannel(Guid.NewGuid(), provider.Id, "Permata Virtual Account", "PERMATA", PaymentMethodType.VirtualAccount, null, 10000, 50000000, 4500, 0),

            // QRIS
            new PaymentChannel(Guid.NewGuid(), provider.Id, "QRIS (DANA/OVO/GoPay/LinkAja)", "QRIS", PaymentMethodType.QrCode, null, 1000, 2000000, 0, 0.7m),

            // eWallets
            new PaymentChannel(Guid.NewGuid(), provider.Id, "OVO", "OVO", PaymentMethodType.EWallet, null, 1000, 10000000, 0, 1.5m),
            new PaymentChannel(Guid.NewGuid(), provider.Id, "DANA", "DANA", PaymentMethodType.EWallet, null, 1000, 10000000, 0, 1.5m),
            new PaymentChannel(Guid.NewGuid(), provider.Id, "ShopeePay", "SHOPEEPAY", PaymentMethodType.EWallet, null, 1000, 10000000, 0, 2.0m),
            new PaymentChannel(Guid.NewGuid(), provider.Id, "GoPay", "GOPAY", PaymentMethodType.EWallet, null, 1000, 10000000, 0, 2.0m)
        };

        context.Set<PaymentChannel>().AddRange(channels);
        await context.SaveChangesAsync();
    }

    private static async Task SeedCreditPackages(ApplicationDbContext context)
    {
        var hasPackages = await context.Set<CreditPackage>().AnyAsync();
        if (hasPackages)
            return;

        var packages = new List<CreditPackage>
        {
            new CreditPackage(
                Guid.NewGuid(),
                "Paket Hemat (50rb)",
                Money.IDR(50000),
                creditAmount: 50000,
                description: "Top Up Kredit Rp 50.000"),

            new CreditPackage(
                Guid.NewGuid(),
                "Paket Populer (100rb)",
                Money.IDR(100000),
                creditAmount: 105000,
                description: "Top Up Kredit Rp 100.000 dengan bonus Rp 5.000"),

            new CreditPackage(
                Guid.NewGuid(),
                "Paket Premium (250rb)",
                Money.IDR(250000),
                creditAmount: 270000,
                description: "Top Up Kredit Rp 250.000 dengan bonus Rp 20.000"),

            new CreditPackage(
                Guid.NewGuid(),
                "Paket Super (500rb)",
                Money.IDR(500000),
                creditAmount: 550000,
                description: "Top Up Kredit Rp 500.000 dengan bonus Rp 50.000")
        };

        context.Set<CreditPackage>().AddRange(packages);
        await context.SaveChangesAsync();
    }
}
