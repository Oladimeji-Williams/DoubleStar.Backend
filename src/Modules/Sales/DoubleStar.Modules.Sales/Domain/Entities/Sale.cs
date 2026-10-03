// Domain/Entities/Sale.cs
using DoubleStar.SharedKernel.Domain;
using DoubleStar.Modules.Sales.Domain.Enums;

namespace DoubleStar.Modules.Sales.Domain.Entities;

public sealed class Sale : Entity
{
    private readonly List<SaleLine> _lines = [];

    public Guid? CustomerId { get; private set; }
    public SaleStatus Status { get; private set; }
    public long TotalKobo { get; private set; }
    public IReadOnlyList<SaleLine> Lines => _lines;

    private Sale() { }

    public static Sale CreateDraft(Guid? customerId) => new()
    {
        CustomerId = customerId,
        Status = SaleStatus.Draft,
    };

    public SaleLine AddLine(int productId, long unitPriceKobo, int quantity, string? serialNumber)
    {
        if (Status != SaleStatus.Draft)
        {
            throw new InvalidOperationException("Cannot add lines to a sale that is not in Draft status.");
        }

        // Bulk items merge into an existing line for the same product — a serialized item
        // never merges, since each one is a distinct physical unit and must stay its own line.
        if (serialNumber is null)
        {
            var existingLine = _lines.FirstOrDefault(l => l.ProductId == productId && l.SerialNumber is null);
            if (existingLine is not null)
            {
                existingLine.IncreaseQuantity(quantity);
                RecalculateTotal();
                return existingLine;
            }
        }

        var line = SaleLine.Create(productId, unitPriceKobo, quantity, serialNumber);
        _lines.Add(line);
        RecalculateTotal();
        return line;
    }

    public void MarkLineDeducted(int lineId)
    {
        var line = _lines.FirstOrDefault(l => l.Id == lineId)
            ?? throw new InvalidOperationException($"Line '{lineId}' does not belong to this sale.");
        line.MarkStockDeducted();
    }

    public void Complete()
    {
        if (Status != SaleStatus.Draft)
        {
            throw new InvalidOperationException("Only a Draft sale can be completed.");
        }
        if (_lines.Count == 0)
        {
            throw new InvalidOperationException("Cannot complete a sale with no lines.");
        }
        Status = SaleStatus.Completed;
    }

    public void Void()
    {
        if (Status == SaleStatus.Completed)
        {
            throw new InvalidOperationException("A completed sale cannot be voided — use a sale return instead.");
        }
        Status = SaleStatus.Void;
    }

    private void RecalculateTotal() => TotalKobo = _lines.Sum(l => l.LineTotalKobo);
}