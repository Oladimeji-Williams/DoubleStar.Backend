// Application/Mappings/SaleMappings.cs
using DoubleStar.Modules.Sales.Application.DTOs;
using DoubleStar.Modules.Sales.Domain.Entities;

namespace DoubleStar.Modules.Sales.Application.Mappings;

public static class SaleMappings
{
    public static SaleLineDto ToDto(this SaleLine line) => new(
        line.Id, line.ProductId, line.UnitPriceKobo, line.Quantity, line.SerialNumber, line.LineTotalKobo, line.IsStockDeducted);

    public static SaleDto ToDto(this Sale sale) => new(
        sale.Id, sale.CustomerId, sale.Status, sale.TotalKobo, sale.Lines.Select(l => l.ToDto()).ToList());
}