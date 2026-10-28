namespace DoubleStar.Modules.Sales.Api.Contracts;

public sealed record AddSaleLineRequest(int ProductId, int? Quantity, string? SerialNumber);