using DoubleStar.SharedKernel.Domain;

namespace DoubleStar.Modules.Orders.Domain.Entities;

/// <summary>Everything the customer agreed to, frozen at checkout — later catalog edits never rewrite history.</summary>
public sealed class OrderLine : Entity
{
    public int OrderId { get; private set; }
    public int ProductId { get; private set; }
    public string ProductName { get; private set; } = null!;
    public string Sku { get; private set; } = null!;
    public long UnitPriceKobo { get; private set; }
    public int Quantity { get; private set; }
    public bool IsSerialized { get; private set; }

    /// <summary>IMEIs/serials reserved for this line (serialized products only).</summary>
    public string[] SerialNumbers { get; private set; } = [];

    public bool IsStockConsumed { get; private set; }

    public long LineTotalKobo => UnitPriceKobo * Quantity;

    private OrderLine() { }

    internal static OrderLine Create(OrderLineDraft draft) => new()
    {
        ProductId = draft.ProductId,
        ProductName = draft.ProductName,
        Sku = draft.Sku,
        UnitPriceKobo = draft.UnitPriceKobo,
        Quantity = draft.Quantity,
        IsSerialized = draft.IsSerialized,
        SerialNumbers = [.. draft.SerialNumbers],
    };

    internal void MarkStockConsumed() => IsStockConsumed = true;
}

public sealed record OrderLineDraft(
    int ProductId,
    string ProductName,
    string Sku,
    long UnitPriceKobo,
    int Quantity,
    bool IsSerialized,
    IReadOnlyList<string> SerialNumbers);