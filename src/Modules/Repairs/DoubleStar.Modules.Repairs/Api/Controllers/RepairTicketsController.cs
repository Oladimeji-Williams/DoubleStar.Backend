// Api/Controllers/RepairTicketsController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using DoubleStar.SharedKernel.Abstractions.Authentication;
using DoubleStar.BuildingBlocks.Infrastructure.Api;
using DoubleStar.Modules.Repairs.Api.Contracts;
using DoubleStar.Modules.Repairs.Application.Commands.AddPartUsedCommand;
using DoubleStar.Modules.Repairs.Application.Commands.ApproveQuoteCommand;
using DoubleStar.Modules.Repairs.Application.Commands.AssignTechnicianCommand;
using DoubleStar.Modules.Repairs.Application.Commands.CancelRepairCommand;
using DoubleStar.Modules.Repairs.Application.Commands.CollectDeviceCommand;
using DoubleStar.Modules.Repairs.Application.Commands.MarkReadyForCollectionCommand;
using DoubleStar.Modules.Repairs.Application.Commands.OpenRepairTicketCommand;
using DoubleStar.Modules.Repairs.Application.Commands.RecordDiagnosisCommand;
using DoubleStar.Modules.Repairs.Application.Queries.GetAllTicketsQuery;
using DoubleStar.Modules.Repairs.Application.Queries.GetTicketByIdQuery;
using DoubleStar.Modules.Repairs.Application.Queries.GetTicketsByCustomerQuery;

namespace DoubleStar.Modules.Repairs.Api.Controllers;

public sealed class RepairTicketsController(ISender sender, ICurrentUser currentUser) : V1ControllerBase
{
    [HttpPost]
    [Authorize(Roles = "Admin,Manager,Cashier,Technician")]
    public async Task<IActionResult> Open(OpenRepairTicketRequest request, CancellationToken cancellationToken)
    {
        var result = await sender.Send(
            new OpenRepairTicketCommand(
                request.CustomerId, request.WalkInName, request.WalkInPhone,
                request.DeviceDescription, request.ImeiOrSerial, request.FaultDescription),
            cancellationToken);
        return result.IsFailure ? Failure(result) : Success(result.Value);
    }

    [HttpPost("{id:int}/diagnosis")]
    [Authorize(Roles = "Admin,Manager,Technician")]
    public async Task<IActionResult> RecordDiagnosis(int id, RecordDiagnosisRequest request, CancellationToken cancellationToken)
    {
        var result = await sender.Send(
            new RecordDiagnosisCommand(id, request.DiagnosisNotes, request.QuotedPriceKobo), cancellationToken);
        return result.IsFailure ? Failure(result) : NoContent();
    }

    [HttpPost("{id:int}/approve-quote")]
    [Authorize(Roles = "Admin,Manager,Cashier")]
    public async Task<IActionResult> ApproveQuote(int id, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new ApproveQuoteCommand(id), cancellationToken);
        return result.IsFailure ? Failure(result) : NoContent();
    }

    [HttpPost("{id:int}/assign-technician")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> AssignTechnician(int id, AssignTechnicianRequest request, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new AssignTechnicianCommand(id, request.TechnicianUserId), cancellationToken);
        return result.IsFailure ? Failure(result) : NoContent();
    }

    [HttpPost("{id:int}/parts")]
    [Authorize(Roles = "Admin,Manager,Technician")]
    public async Task<IActionResult> AddPart(int id, AddPartUsedRequest request, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new AddPartUsedCommand(id, request.ProductId, request.Quantity), cancellationToken);
        return result.IsFailure ? Failure(result) : Success(result.Value);
    }

    [HttpPost("{id:int}/ready")]
    [Authorize(Roles = "Admin,Manager,Technician")]
    public async Task<IActionResult> MarkReady(int id, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new MarkReadyForCollectionCommand(id), cancellationToken);
        return result.IsFailure ? Failure(result) : NoContent();
    }

    [HttpPost("{id:int}/collect")]
    [Authorize(Roles = "Admin,Manager,Cashier")]
    public async Task<IActionResult> Collect(int id, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new CollectDeviceCommand(id), cancellationToken);
        return result.IsFailure ? Failure(result) : NoContent();
    }

    [HttpPost("{id:int}/cancel")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> Cancel(int id, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new CancelRepairCommand(id), cancellationToken);
        return result.IsFailure ? Failure(result) : NoContent();
    }

    [HttpGet("{id:int}")]
    [Authorize(Roles = "Admin,Manager,Cashier,Technician")]
    public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetTicketByIdQuery(id), cancellationToken);
        return result.IsFailure ? Failure(result) : Success(result.Value);
    }

    [HttpGet]
    [Authorize(Roles = "Admin,Manager,Cashier,Technician")]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetAllTicketsQuery(), cancellationToken);
        return result.IsFailure ? Failure(result) : Success(result.Value);
    }

    [HttpGet("me")]
    [Authorize(Roles = "Customer")]
    public async Task<IActionResult> GetMine(CancellationToken cancellationToken)
    {
        if (currentUser.UserId is null)
        {
            return Unauthorized();
        }

        var result = await sender.Send(new GetTicketsByCustomerQuery(currentUser.UserId.Value), cancellationToken);
        return result.IsFailure ? Failure(result) : Success(result.Value);
    }
}