// Application/DTOs/SaleDto.cs
using DoubleStar.Modules.Sales.Domain.Enums;

namespace DoubleStar.Modules.Sales.Application.DTOs;

public sealed record SaleLineDto(
    int Id, int ProductId, long UnitPriceKobo, int Quantity, string? SerialNumber, long LineTotalKobo, bool IsStockDeducted);

public sealed record SaleDto(
    int Id, Guid? CustomerId, SaleStatus Status, long TotalKobo, IReadOnlyList<SaleLineDto> Lines);