// Domain/Enums/PaymentStatus.cs
namespace DoubleStar.Modules.Payments.Domain.Enums;

public enum PaymentStatus
{
    Pending,
    Successful,
    Failed,
    Refunded,
    PartiallyRefunded
}