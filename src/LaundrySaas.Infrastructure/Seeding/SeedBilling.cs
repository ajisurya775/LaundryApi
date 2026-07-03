using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using LaundrySaas.Domain.Billing;
using LaundrySaas.Infrastructure.Persistence;
using LaundrySaas.SharedKernel.ValueObjects;

namespace LaundrySaas.Infrastructure.Seeding;

public static class SeedBilling
{
    public static async Task SeedAsync(ApplicationDbContext context)
    {
        await SeedPaymentGateway(context);
        await SeedSubscriptionPlans(context);
    }

    private static async Task<PaymentGateway> SeedPaymentGateway(ApplicationDbContext context)
    {
        var xenditGateway = await context.PaymentGateways
            .FirstOrDefaultAsync(p => p.Code == "XENDIT");

        if (xenditGateway == null)
        {
            xenditGateway = new PaymentGateway(
                Guid.NewGuid(),
                "Xendit Payment Gateway",
                "XENDIT",
                "Integrasi Xendit untuk pembayaran otomatis via Virtual Account, eWallet, dan QRIS");

            context.PaymentGateways.Add(xenditGateway);
            await context.SaveChangesAsync();
        }

        return xenditGateway;
    }

    private static async Task SeedSubscriptionPlans(ApplicationDbContext context)
    {
        var hasPlans = await context.SubscriptionPlans.AnyAsync();
        if (hasPlans)
            return;

        var plans = new List<SubscriptionPlan>
        {
            new SubscriptionPlan(
                Guid.Parse("11111111-1111-1111-1111-111111111111"),
                "Basic Plan",
                Money.IDR(99000),
                hasPos: true,
                hasInventory: false,
                hasAccounting: false,
                extraCredit: 0),

            new SubscriptionPlan(
                Guid.Parse("22222222-2222-2222-2222-222222222222"),
                "Pro Plan",
                Money.IDR(249000),
                hasPos: true,
                hasInventory: true,
                hasAccounting: false,
                extraCredit: 25000),

            new SubscriptionPlan(
                Guid.Parse("33333333-3333-3333-3333-333333333333"),
                "Enterprise Plan",
                Money.IDR(499000),
                hasPos: true,
                hasInventory: true,
                hasAccounting: true,
                extraCredit: 100000)
        };

        context.SubscriptionPlans.AddRange(plans);
        await context.SaveChangesAsync();
    }
}
