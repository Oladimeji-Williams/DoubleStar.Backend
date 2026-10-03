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
using DoubleStar.Modules.Catalog.Application.Queries.GetArchivedProductsQuery;
using DoubleStar.Modules.Catalog.Application.Commands.UnarchiveProductCommand;
using Microsoft.AspNetCore.Hosting;
using DoubleStar.Modules.Catalog.Application.Commands.SetProductImageCommand;
using Microsoft.AspNetCore.Http;
using DoubleStar.Modules.Catalog.Application.Queries.GetPagedProductsQuery;

namespace DoubleStar.Modules.Catalog.Api.Controllers;

public sealed class ProductsController(ISender sender, IWebHostEnvironment environment) : V1ControllerBase
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

    [HttpGet("archived")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> GetArchived(CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetArchivedProductsQuery(), cancellationToken);
        return result.IsFailure ? Failure(result) : Success(result.Value);
    }

    [HttpPost("{id:int}/unarchive")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> Unarchive(int id, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new UnarchiveProductCommand(id), cancellationToken);
        return result.IsFailure ? Failure(result) : NoContent();
    }

    [HttpPost("{id:int}/image")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> UploadImage(int id, IFormFile file, CancellationToken cancellationToken)
    {
        if (file is null || file.Length == 0)
            return Failure(Result.Failure(new Error("Image.Empty", "No file was uploaded.", ErrorType.Validation)));

        var allowedExtensions = new HashSet<string> { ".jpg", ".jpeg", ".png", ".webp" };
        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!allowedExtensions.Contains(extension))
            return Failure(Result.Failure(new Error("Image.InvalidType", "Only JPG, PNG or WEBP images are allowed.", ErrorType.Validation)));

        if (file.Length > 5 * 1024 * 1024)
            return Failure(Result.Failure(new Error("Image.TooLarge", "Images must be 5MB or smaller.", ErrorType.Validation)));

        var webRoot = environment.WebRootPath ?? Path.Combine(environment.ContentRootPath, "wwwroot");
        var uploadsPath = Path.Combine(webRoot, "uploads", "products");
        Directory.CreateDirectory(uploadsPath);

        var fileName = $"{Guid.NewGuid()}{extension}";
        var filePath = Path.Combine(uploadsPath, fileName);

        await using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(stream, cancellationToken);
        }

        var result = await sender.Send(new SetProductImageCommand(id, $"/uploads/products/{fileName}"), cancellationToken);
        return result.IsFailure ? Failure(result) : Success(result.Value);
    }

    [HttpDelete("{id:int}/image")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> RemoveImage(int id, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new SetProductImageCommand(id, null), cancellationToken);
        return result.IsFailure ? Failure(result) : Success(result.Value);
    }

    [HttpGet("paged")]
    [AllowAnonymous]
    public async Task<IActionResult> GetPaged([FromQuery] int page = 1, [FromQuery] int pageSize = 12, CancellationToken cancellationToken = default)
    {
        var result = await sender.Send(new GetPagedProductsQuery(page, pageSize), cancellationToken);
        return result.IsFailure ? Failure(result) : Success(result.Value);
    }
}