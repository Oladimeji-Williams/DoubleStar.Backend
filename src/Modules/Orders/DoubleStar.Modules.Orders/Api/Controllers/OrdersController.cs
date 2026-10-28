using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using DoubleStar.BuildingBlocks.Application.Common.Queries.Pagination;
using DoubleStar.BuildingBlocks.Infrastructure.Api;
using DoubleStar.Modules.Orders.Api.Contracts;
using DoubleStar.Modules.Orders.Application.Commands.CancelOrder;
using DoubleStar.Modules.Orders.Application.Commands.RetryOrderPayment;
using DoubleStar.Modules.Orders.Application.Commands.UpdateOrderStatus;
using DoubleStar.Modules.Orders.Application.Queries.GetAllOrders;
using DoubleStar.Modules.Orders.Application.Queries.GetMyOrders;
using DoubleStar.Modules.Orders.Application.Queries.GetOrderById;

namespace DoubleStar.Modules.Orders.Api.Controllers;

/// <summary>/api/v1/orders — order history, payment retry and staff fulfilment.</summary>
public sealed class OrdersController(ISender sender) : V1ControllerBase
{
    private const string StaffRoles = "Admin,Manager,Cashier";

    // Customers may only touch their own orders and staff may touch any; the handlers enforce
    // ownership and answer "not found" for someone else's order.
    private const string OrderViewerRoles = "Admin,Manager,Cashier,Customer";

    /// <summary>The signed-in customer's own orders.</summary>
    [HttpGet("mine")]
    [Authorize(Roles = "Customer")]
    public async Task<IActionResult> GetMine(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 25,
        [FromQuery] string? search = null,
        [FromQuery] string? sortBy = null,
        [FromQuery] bool sortDescending = false,
        [FromQuery] string? filters = null,
        CancellationToken cancellationToken = default)
    {
        if (!PagedQueryFactory.TryCreate(page, pageSize, search, sortBy, sortDescending, filters, out var query))
        {
            return InvalidFilters();
        }

        var result = await sender.Send(new GetMyOrdersQuery(query), cancellationToken);
        return result.IsFailure ? Failure(result) : Success(result.Value);
    }

    /// <summary>Every order (staff).</summary>
    [HttpGet]
    [Authorize(Roles = StaffRoles)]
    public async Task<IActionResult> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 25,
        [FromQuery] string? search = null,
        [FromQuery] string? sortBy = null,
        [FromQuery] bool sortDescending = false,
        [FromQuery] string? filters = null,
        CancellationToken cancellationToken = default)
    {
        if (!PagedQueryFactory.TryCreate(page, pageSize, search, sortBy, sortDescending, filters, out var query))
        {
            return InvalidFilters();
        }

        var result = await sender.Send(new GetAllOrdersQuery(query), cancellationToken);
        return result.IsFailure ? Failure(result) : Success(result.Value);
    }

    [HttpGet("{id:int}")]
    [Authorize(Roles = OrderViewerRoles)]
    public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetOrderByIdQuery(id), cancellationToken);
        return result.IsFailure ? Failure(result) : Success(result.Value);
    }

    /// <summary>Cancels an order that is still awaiting payment.</summary>
    [HttpPost("{id:int}/cancel")]
    [Authorize(Roles = OrderViewerRoles)]
    public async Task<IActionResult> Cancel(int id, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new CancelOrderCommand(id), cancellationToken);
        return result.IsFailure ? Failure(result) : NoContent();
    }

    /// <summary>"Complete payment" for an order still awaiting payment. Returns a fresh Paystack URL.</summary>
    [HttpPost("{id:int}/retry-payment")]
    [Authorize(Roles = "Customer")]
    public async Task<IActionResult> RetryPayment(int id, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new RetryOrderPaymentCommand(id), cancellationToken);
        return result.IsFailure ? Failure(result) : Success(result.Value);
    }

    /// <summary>Staff fulfilment progress (processing, ready for pickup, out for delivery, completed).</summary>
    [HttpPut("{id:int}/status")]
    [Authorize(Roles = StaffRoles)]
    public async Task<IActionResult> UpdateStatus(
        int id, UpdateOrderStatusRequest request, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new UpdateOrderStatusCommand(id, request.Status), cancellationToken);
        return result.IsFailure ? Failure(result) : NoContent();
    }

    private IActionResult InvalidFilters() =>
        Failure(Result.Failure(new Error("Validation.Filters", "The filters value is not valid JSON.", ErrorType.Validation)));
}