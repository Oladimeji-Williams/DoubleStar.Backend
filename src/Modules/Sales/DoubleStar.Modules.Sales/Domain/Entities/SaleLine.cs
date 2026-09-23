// Domain/Entities/SaleLine.cs
using DoubleStar.SharedKernel.Domain;

namespace DoubleStar.Modules.Sales.Domain.Entities;

public sealed class SaleLine : Entity
{
    public int SaleId { get; private set; }
    public int ProductId { get; private set; }
    public long UnitPriceKobo { get; private set; }
    public int Quantity { get; private set; }
    public string? SerialNumber { get; private set; }
    public bool IsStockDeducted { get; private set; }

    public long LineTotalKobo => UnitPriceKobo * Quantity;

    private SaleLine() { }

    internal static SaleLine Create(int productId, long unitPriceKobo, int quantity, string? serialNumber) => new()
    {
        ProductId = productId,
        UnitPriceKobo = unitPriceKobo,
        Quantity = quantity,
        SerialNumber = serialNumber?.Trim().ToUpperInvariant(),
    };

    internal void MarkStockDeducted() => IsStockDeducted = true;
}