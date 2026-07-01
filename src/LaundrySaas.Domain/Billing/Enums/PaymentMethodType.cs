namespace LaundrySaas.Domain.Billing;

/// <summary>
/// Generic payment method types supported across all payment gateway providers.
/// </summary>
public enum PaymentMethodType
{
    VirtualAccount,   // Transfer bank via Virtual Account
    EWallet,          // Digital wallet (OVO, DANA, GoPay, ShopeePay, dll)
    QrCode,           // QRIS / QR payment
    RetailOutlet,     // Pembayaran di minimarket (Alfamart, Indomaret)
    BankTransfer,     // Transfer bank langsung
    CreditCard        // Kartu kredit
}
