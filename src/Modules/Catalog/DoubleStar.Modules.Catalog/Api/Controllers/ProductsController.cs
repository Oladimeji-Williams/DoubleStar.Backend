// Api/Controllers/ProductsController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using DoubleStar.BuildingBlocks.Infrastructure.Api;
using DoubleStar.Modules.Catalog.Api.Contracts;
using DoubleStar.Modules.Catalog.Application.Commands.ArchiveProductCommand;
using DoubleStar.Modules.Catalog.Application.Commands.CreateProductCommand;
using DoubleStar.Modules.Catalog.Application.Commands.SetProductPriceCommand;
using DoubleStar.Modules.Catalog.Application.Commands.UpdateProductCommand;
using DoubleStar.Modules.Catalog.Application.Queries.GetAllProductsQuery;
using DoubleStar.Modules.Catalog.Application.Queries.GetProductByIdQuery;
using DoubleStar.Modules.Catalog.Application.Queries.SearchProductsQuery;

namespace DoubleStar.Modules.Catalog.Api.Controllers;

public sealed class ProductsController(ISender sender) : V1ControllerBase
{
    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetAllProductsQuery(), cancellationToken);
        return result.IsFailure ? Failure(result) : Success(result.Value);
    }

    [HttpGet("search")]
    [AllowAnonymous]
    public async Task<IActionResult> Search([FromQuery] string term, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new SearchProductsQuery(term), cancellationToken);
        return result.IsFailure ? Failure(result) : Success(result.Value);
    }

    [HttpGet("{id:int}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetProductByIdQuery(id), cancellationToken);
        return result.IsFailure ? Failure(result) : Success(result.Value);
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> Create(CreateProductRequest request, CancellationToken cancellationToken)
    {
        var result = await sender.Send(
            new CreateProductCommand(
                request.Name, request.Sku, request.Description, request.CategoryId, request.BrandId,
                request.UnitPriceKobo, request.TrackingMode),
            cancellationToken);
        return result.IsFailure ? Failure(result) : Success(result.Value);
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> Update(int id, UpdateProductRequest request, CancellationToken cancellationToken)
    {
        var result = await sender.Send(
            new UpdateProductCommand(id, request.Name, request.Description, request.CategoryId, request.BrandId),
            cancellationToken);
        return result.IsFailure ? Failure(result) : NoContent();
    }

    [HttpPut("{id:int}/price")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> SetPrice(int id, SetProductPriceRequest request, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new SetProductPriceCommand(id, request.UnitPriceKobo), cancellationToken);
        return result.IsFailure ? Failure(result) : NoContent();
    }

    [HttpPost("{id:int}/archive")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> Archive(int id, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new ArchiveProductCommand(id), cancellationToken);
        return result.IsFailure ? Failure(result) : NoContent();
    }
}