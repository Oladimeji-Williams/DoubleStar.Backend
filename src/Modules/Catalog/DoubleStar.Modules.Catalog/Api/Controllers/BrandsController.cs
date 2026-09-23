// Api/Controllers/BrandsController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using DoubleStar.BuildingBlocks.Infrastructure.Api;
using DoubleStar.Modules.Catalog.Api.Contracts;
using DoubleStar.Modules.Catalog.Application.Commands.CreateBrandCommand;
using DoubleStar.Modules.Catalog.Application.Queries.GetAllBrandsQuery;

namespace DoubleStar.Modules.Catalog.Api.Controllers;

public sealed class BrandsController(ISender sender) : V1ControllerBase
{
    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetAllBrandsQuery(), cancellationToken);
        return result.IsFailure ? Failure(result) : Success(result.Value);
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> Create(CreateLookupRequest request, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new CreateBrandCommand(request.Name), cancellationToken);
        return result.IsFailure ? Failure(result) : Success(result.Value);
    }
}