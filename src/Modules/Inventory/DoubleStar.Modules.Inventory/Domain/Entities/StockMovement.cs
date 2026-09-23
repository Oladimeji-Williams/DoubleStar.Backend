// Domain/Entities/StockMovement.cs
using DoubleStar.SharedKernel.Domain;
using DoubleStar.Modules.Inventory.Domain.Enums;

namespace DoubleStar.Modules.Inventory.Domain.Entities;

/// <summary>Immutable audit trail row — never updated after creation.</summary>
public sealed class StockMovement : Entity
{
    public int ProductId { get; private set; }
    public StockMovementType MovementType { get; private set; }
    public int Quantity { get; private set; }
    public string? Reference { get; private set; }

    private StockMovement() { }

    public static StockMovement Create(int productId, StockMovementType movementType, int quantity, string? reference) => new()
    {
        ProductId = productId,
        MovementType = movementType,
        Quantity = quantity,
        Reference = reference?.Trim(),
    };
}