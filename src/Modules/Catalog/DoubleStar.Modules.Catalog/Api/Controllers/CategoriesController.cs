// Api/Controllers/CategoriesController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using DoubleStar.BuildingBlocks.Infrastructure.Api;
using DoubleStar.Modules.Catalog.Api.Contracts;
using DoubleStar.Modules.Catalog.Application.Commands.CreateCategoryCommand;
using DoubleStar.Modules.Catalog.Application.Queries.GetAllCategoriesQuery;

namespace DoubleStar.Modules.Catalog.Api.Controllers;

public sealed class CategoriesController(ISender sender) : V1ControllerBase
{
    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetAllCategoriesQuery(), cancellationToken);
        return result.IsFailure ? Failure(result) : Success(result.Value);
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> Create(CreateLookupRequest request, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new CreateCategoryCommand(request.Name), cancellationToken);
        return result.IsFailure ? Failure(result) : Success(result.Value);
    }
}