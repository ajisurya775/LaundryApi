namespace LaundrySaas.Domain.Billing;

public enum OrderStatus
{
    Pending,      // Menunggu pembayaran dari user (untuk top-up)
    Processing,   // Sedang diproses oleh payment gateway
    Succeeded,    // Order berhasil, credit sudah ditambahkan atau dipotong
    Failed,       // Order/pembayaran gagal
    Expired,      // Pembayaran expired
    Cancelled     // Dibatalkan oleh user/admin
}
