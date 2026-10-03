// Api/Controllers/StaffController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using DoubleStar.BuildingBlocks.Infrastructure.Api;
using DoubleStar.Modules.Identity.Api.Contracts;
using DoubleStar.Modules.Identity.Application.Commands.DeactivateStaffCommand;
using DoubleStar.Modules.Identity.Application.Commands.RegisterStaffCommand;
using DoubleStar.Modules.Identity.Application.Queries.GetAllStaffQuery;
using DoubleStar.Modules.Identity.Application.Commands.UpdateStaffCommand;
using DoubleStar.Modules.Identity.Application.Commands.ReactivateStaffCommand;


namespace DoubleStar.Modules.Identity.Api.Controllers;

[Authorize(Roles = "Admin")]
public sealed class StaffController(ISender sender) : V1ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> CreateStaff(CreateStaffRequest request, CancellationToken cancellationToken)
    {
        var result = await sender.Send(
            new RegisterStaffCommand(request.FirstName, request.LastName, request.Email, request.Password, request.Role),
            cancellationToken);
        return result.IsFailure ? Failure(result) : Success(result.Value);
    }

    [HttpPost("{id:guid}/deactivate")]
    public async Task<IActionResult> Deactivate(Guid id, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new DeactivateStaffCommand(id), cancellationToken);
        return result.IsFailure ? Failure(result) : NoContent();
    }

    [HttpGet]
    public async Task<IActionResult> GetAllStaff(CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetAllStaffQuery(), cancellationToken);
        return result.IsFailure ? Failure(result) : Success(result.Value);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdateStaff(Guid id, UpdateStaffRequest request, CancellationToken cancellationToken)
    {
        var result = await sender.Send(
            new UpdateStaffCommand(id, request.FirstName, request.LastName, request.Role), cancellationToken);
        return result.IsFailure ? Failure(result) : NoContent();
    }

    [HttpPost("{id:guid}/reactivate")]
    public async Task<IActionResult> Reactivate(Guid id, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new ReactivateStaffCommand(id), cancellationToken);
        return result.IsFailure ? Failure(result) : NoContent();
    }
}