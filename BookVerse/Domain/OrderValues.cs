namespace BookVerse.Domain;

public static class OrderValues
{
    public static class Status
    {
        public const string Pending = "Chờ xử lý";
        public const string Shipping = "Đang giao";
        public const string Delivered = "Đã giao";
        public const string Cancelled = "Đã hủy";
    }

    public static class PaymentMethod
    {
        public const string Cod = "COD";
        public const string VnPay = "VNPAY";
    }

    public static class PaymentStatus
    {
        public const string Unpaid = "Chưa thanh toán";
        public const string PendingVnPay = "Chờ thanh toán VNPay";
        public const string Paid = "Đã thanh toán";
        public const string Failed = "Thanh toán thất bại";
        public const string Cancelled = "Đã hủy";
    }
}
