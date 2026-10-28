namespace DoubleStar.SharedKernel.Contracts.Orders;

public enum OrderStatus
{
    PendingPayment,
    Paid,
    Processing,
    ReadyForPickup,
    OutForDelivery,
    Completed,
    Cancelled,
    Expired
}
