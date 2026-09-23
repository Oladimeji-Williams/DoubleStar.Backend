// Domain/Enums/StockMovementType.cs
namespace DoubleStar.Modules.Inventory.Domain.Enums;

public enum StockMovementType
{
    In,
    Out,
    Adjustment,
    Reservation,
    ReservationRelease
}