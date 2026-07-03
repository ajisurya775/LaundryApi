namespace LaundrySaas.Domain.Billing;

/// <summary>
/// Payment method types yang didukung.
/// Selaras dengan channel yang disediakan oleh Xendit payment gateway.
/// </summary>
public enum PaymentMethodType
{
    Cash,             // Pembayaran tunai (offline, tanpa gateway)
    VirtualAccount,   // Transfer bank via Virtual Account (BCA VA, BNI VA, Mandiri VA, dll)
    EWallet,          // Digital wallet (OVO, DANA, GoPay, ShopeePay, LinkAja)
    QrCode,           // QRIS / QR payment
    RetailOutlet,     // Pembayaran di minimarket (Alfamart, Indomaret)
    BankTransfer,     // Transfer bank langsung / direct debit
    CreditCard,       // Kartu kredit / debit
    PayLater          // Paylater (Kredivo, Akulaku, dll)
}
