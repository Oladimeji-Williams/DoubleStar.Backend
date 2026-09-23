// Api/Controllers/StaffController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using DoubleStar.BuildingBlocks.Infrastructure.Api;
using DoubleStar.Modules.Identity.Api.Contracts;
using DoubleStar.Modules.Identity.Application.Commands.DeactivateStaffCommand;
using DoubleStar.Modules.Identity.Application.Commands.RegisterStaffCommand;

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
}