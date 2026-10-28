using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using DoubleStar.BuildingBlocks.Infrastructure.Api;
using DoubleStar.Modules.Orders.Application.Commands.AddCartItem;
using DoubleStar.Modules.Orders.Application.Commands.RemoveCartItem;
using DoubleStar.Modules.Orders.Application.Commands.SetCartItemQuantity;
using DoubleStar.Modules.Orders.Application.Queries.GetMyCart;

namespace DoubleStar.Modules.Orders.Api.Controllers;

[Authorize(Roles = "Customer")]
public sealed class CartController(ISender sender) : V1ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get(CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetMyCartQuery(), cancellationToken);
        return result.IsFailure ? Failure(result) : Success(result.Value);
    }

    [HttpPost("items")]
    public async Task<IActionResult> AddItem(AddCartItemCommand request, CancellationToken cancellationToken)
    {
        var result = await sender.Send(request, cancellationToken);
        return result.IsFailure ? Failure(result) : Success(result.Value);
    }

    [HttpPut("items/{productId:int}")]
    public async Task<IActionResult> SetItemQuantity(
        int productId,
        SetCartItemQuantityRequest request,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(
            new SetCartItemQuantityCommand(productId, request.Quantity),
            cancellationToken);
        return result.IsFailure ? Failure(result) : Success(result.Value);
    }

    [HttpDelete("items/{productId:int}")]
    public async Task<IActionResult> RemoveItem(int productId, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new RemoveCartItemCommand(productId), cancellationToken);
        return result.IsFailure ? Failure(result) : Success(result.Value);
    }
}

public sealed record SetCartItemQuantityRequest(int Quantity);
