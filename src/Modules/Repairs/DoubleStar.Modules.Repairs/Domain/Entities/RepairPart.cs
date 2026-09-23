// Domain/Entities/RepairPart.cs
using DoubleStar.SharedKernel.Domain;

namespace DoubleStar.Modules.Repairs.Domain.Entities;

public sealed class RepairPart : Entity
{
    public int RepairTicketId { get; private set; }
    public int ProductId { get; private set; }
    public int Quantity { get; private set; }
    public long UnitCostKobo { get; private set; }

    public long TotalCostKobo => UnitCostKobo * Quantity;

    private RepairPart() { }

    internal static RepairPart Create(int productId, int quantity, long unitCostKobo) => new()
    {
        ProductId = productId,
        Quantity = quantity,
        UnitCostKobo = unitCostKobo,
    };
}