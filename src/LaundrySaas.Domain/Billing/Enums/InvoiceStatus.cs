namespace LaundrySaas.Domain.Billing;

public enum InvoiceStatus
{
    Draft,       // Invoice baru dibuat, belum final
    Unpaid,      // Sudah diterbitkan, menunggu pembayaran
    PartialPaid, // Sebagian sudah dibayar
    Paid,        // Lunas
    Void,        // Dibatalkan
    Overdue      // Lewat jatuh tempo
}
