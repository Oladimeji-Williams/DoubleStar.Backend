// Api/Controllers/SalesController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using DoubleStar.SharedKernel.Abstractions.Authentication;
using DoubleStar.BuildingBlocks.Infrastructure.Api;
using DoubleStar.Modules.Sales.Api.Contracts;
using DoubleStar.Modules.Sales.Application.Commands.AddSaleLineCommand;
using DoubleStar.Modules.Sales.Application.Commands.CompleteSaleCommand;
using DoubleStar.Modules.Sales.Application.Commands.CreateSaleCommand;
using DoubleStar.Modules.Sales.Application.Commands.VoidSaleCommand;
using DoubleStar.Modules.Sales.Application.Queries.GetAllSalesQuery;
using DoubleStar.Modules.Sales.Application.Queries.GetSaleByIdQuery;
using DoubleStar.Modules.Sales.Application.Queries.GetSalesByCustomerQuery;

namespace DoubleStar.Modules.Sales.Api.Controllers;

public sealed class SalesController(ISender sender, ICurrentUser currentUser) : V1ControllerBase
{
    [HttpPost]
    [Authorize(Roles = "Admin,Manager,Cashier")]
    public async Task<IActionResult> Create(CreateSaleRequest request, CancellationToken cancellationToken)
    {
        var result = await sender.Send(
            new CreateSaleCommand(request.CustomerId, request.WalkInName, request.WalkInPhone), cancellationToken);
        return result.IsFailure ? Failure(result) : Success(result.Value);
    }

    [HttpPost("{id:int}/lines")]
    [Authorize(Roles = "Admin,Manager,Cashier")]
    public async Task<IActionResult> AddLine(int id, AddSaleLineRequest request, CancellationToken cancellationToken)
    {
        var result = await sender.Send(
            new AddSaleLineCommand(id, request.ProductId, request.Quantity, request.SerialNumber), cancellationToken);
        return result.IsFailure ? Failure(result) : Success(result.Value);
    }

    [HttpPost("{id:int}/complete")]
    [Authorize(Roles = "Admin,Manager,Cashier")]
    public async Task<IActionResult> Complete(int id, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new CompleteSaleCommand(id), cancellationToken);
        return result.IsFailure ? Failure(result) : Success(result.Value);
    }

    [HttpPost("{id:int}/void")]
    [Authorize(Roles = "Admin,Manager,Cashier")]
    public async Task<IActionResult> Void(int id, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new VoidSaleCommand(id), cancellationToken);
        return result.IsFailure ? Failure(result) : NoContent();
    }

    [HttpGet("{id:int}")]
    [Authorize(Roles = "Admin,Manager,Cashier")]
    public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetSaleByIdQuery(id), cancellationToken);
        return result.IsFailure ? Failure(result) : Success(result.Value);
    }

    [HttpGet]
    [Authorize(Roles = "Admin,Manager,Cashier")]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetAllSalesQuery(), cancellationToken);
        return result.IsFailure ? Failure(result) : Success(result.Value);
    }

    [HttpGet("customers/{customerId:guid}")]
    [Authorize(Roles = "Admin,Manager,Cashier")]
    public async Task<IActionResult> GetByCustomer(Guid customerId, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetSalesByCustomerQuery(customerId), cancellationToken);
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

        var result = await sender.Send(new GetSalesByCustomerQuery(currentUser.UserId.Value), cancellationToken);
        return result.IsFailure ? Failure(result) : Success(result.Value);
    }
}