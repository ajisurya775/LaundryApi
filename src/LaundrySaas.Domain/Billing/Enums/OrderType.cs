namespace LaundrySaas.Domain.Billing;

public enum OrderType
{
    TopUp,           // Pengisian saldo kredit
    UsageFee,        // Pemotongan otomatis per penggunaan fitur
    Subscription,    // Pemotongan langganan bulanan/berkala
    Refund,          // Pengembalian saldo
    Adjustment       // Penyesuaian saldo manual oleh admin
}
