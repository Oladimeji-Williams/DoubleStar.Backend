// Domain/Entities/StockItem.cs
using DoubleStar.SharedKernel.Domain;

namespace DoubleStar.Modules.Inventory.Domain.Entities;

/// <summary>Running quantity for a Bulk-tracked product. One row per product.</summary>
public sealed class StockItem : Entity
{
    public int ProductId { get; private set; }
    public int QuantityOnHand { get; private set; }
    public int QuantityReserved { get; private set; }

    public int QuantityAvailable => QuantityOnHand - QuantityReserved;

    private StockItem() { }

    public static StockItem CreateEmpty(int productId) => new() { ProductId = productId };

    public void Receive(int quantity)
    {
        if (quantity <= 0) throw new ArgumentOutOfRangeException(nameof(quantity));
        QuantityOnHand += quantity;
    }

    public void SetOnHand(int newQuantity)
    {
        if (newQuantity < 0) throw new ArgumentOutOfRangeException(nameof(newQuantity));
        if (newQuantity < QuantityReserved)
        {
            throw new InvalidOperationException(
                "Cannot set on-hand quantity below what is currently reserved.");
        }
        QuantityOnHand = newQuantity;
    }

    public bool TryReserve(int quantity)
    {
        if (quantity <= 0) throw new ArgumentOutOfRangeException(nameof(quantity));
        if (QuantityAvailable < quantity) return false;
        QuantityReserved += quantity;
        return true;
    }

    public void ReleaseReservation(int quantity)
    {
        if (quantity <= 0) throw new ArgumentOutOfRangeException(nameof(quantity));
        QuantityReserved = Math.Max(0, QuantityReserved - quantity);
    }

    public bool TryDeduct(int quantity)
    {
        if (quantity <= 0) throw new ArgumentOutOfRangeException(nameof(quantity));
        if (QuantityOnHand < quantity) return false;
        QuantityOnHand -= quantity;
        QuantityReserved = Math.Max(0, QuantityReserved - quantity);
        return true;
    }
}