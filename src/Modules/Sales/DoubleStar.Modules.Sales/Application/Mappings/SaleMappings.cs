// Modules/Sales/Application/Mappings/SaleMappings.cs — full replacement
using DoubleStar.Modules.Sales.Application.DTOs;
using DoubleStar.Modules.Sales.Domain.Entities;

namespace DoubleStar.Modules.Sales.Application.Mappings;

public static class SaleMappings
{
    // ProductName defaults to a placeholder here since this mapping has no access
    // to the Catalog module — SaleLineEnricher (below) fills in the real name at
    // the Application layer, where cross-module contracts are allowed.
    public static SaleLineDto ToDto(this SaleLine line) => new(
        line.Id, line.ProductId, line.UnitPriceKobo, line.Quantity, line.SerialNumber, line.LineTotalKobo,
        line.IsStockDeducted, $"Product #{line.ProductId}");

    public static SaleDto ToDto(this Sale sale) => new(
        sale.Id, sale.CustomerId, sale.Status, sale.TotalKobo, sale.Lines.Select(l => l.ToDto()).ToList());
}