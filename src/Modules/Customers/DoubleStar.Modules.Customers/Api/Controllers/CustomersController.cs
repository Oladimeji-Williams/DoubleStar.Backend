// Api/Controllers/CustomersController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using DoubleStar.BuildingBlocks.Infrastructure.Api;
using DoubleStar.Modules.Customers.Api.Contracts;
using DoubleStar.Modules.Customers.Application.Commands.CreateWalkInCustomerCommand;
using DoubleStar.Modules.Customers.Application.Commands.UpdateCustomerCommand;
using DoubleStar.Modules.Customers.Application.Queries.GetCustomerByIdQuery;
using DoubleStar.Modules.Customers.Application.Queries.SearchCustomersQuery;

namespace DoubleStar.Modules.Customers.Api.Controllers;

[Authorize(Roles = "Admin,Manager,Cashier")]
public sealed class CustomersController(ISender sender) : V1ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> CreateWalkIn(CreateWalkInCustomerRequest request, CancellationToken cancellationToken)
    {
        var result = await sender.Send(
            new CreateWalkInCustomerCommand(request.Name, request.Phone, request.Email), cancellationToken);
        return result.IsFailure ? Failure(result) : Success(result.Value);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetCustomerByIdQuery(id), cancellationToken);
        return result.IsFailure ? Failure(result) : Success(result.Value);
    }

    [HttpGet("search")]
    public async Task<IActionResult> Search([FromQuery] string term, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new SearchCustomersQuery(term), cancellationToken);
        return result.IsFailure ? Failure(result) : Success(result.Value);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, UpdateCustomerRequest request, CancellationToken cancellationToken)
    {
        var result = await sender.Send(
            new UpdateCustomerCommand(id, request.Name, request.Phone, request.Email, request.Address), cancellationToken);
        return result.IsFailure ? Failure(result) : NoContent();
    }
}