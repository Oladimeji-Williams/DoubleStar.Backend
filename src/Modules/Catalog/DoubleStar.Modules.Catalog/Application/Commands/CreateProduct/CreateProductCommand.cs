// Application/Commands/CreateProductCommand/CreateProductCommand.cs
using DoubleStar.SharedKernel.Contracts.Catalog;
using DoubleStar.Modules.Catalog.Application.DTOs;

namespace DoubleStar.Modules.Catalog.Application.Commands.CreateProductCommand;

public sealed record CreateProductCommand(
    string Name, string Sku, string? Description, int? CategoryId, int? BrandId,
    long UnitPriceKobo, StockTrackingMode TrackingMode) : IRequest<Result<ProductDto>>;