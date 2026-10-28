using DoubleStar.SharedKernel.Abstractions.Authentication;
using DoubleStar.SharedKernel.Contracts.Catalog;
using DoubleStar.SharedKernel.Contracts.Identity;
using DoubleStar.Modules.Orders.Application.Abstractions;
using DoubleStar.Modules.Orders.Application.DTOs;
using DoubleStar.Modules.Orders.Application.Errors;
using DoubleStar.Modules.Orders.Application.Services;
using DoubleStar.Modules.Orders.Domain.Entities;

namespace DoubleStar.Modules.Orders.Application.Commands.AddCartItem;

public sealed class AddCartItemCommandHandler(
    ICartRepository cartRepository,
    ICurrentUser currentUser,
    IProductCatalog productCatalog,
    CartViewBuilder cartViewBuilder)
    : IRequestHandler<AddCartItemCommand, Result<CartDto>>
{
    public async Task<Result<CartDto>> Handle(AddCartItemCommand request, CancellationToken cancellationToken)
    {
        if (currentUser.UserId is not Guid customerId)
        {
            return Result<CartDto>.Failure(UserErrors.NotAuthenticated());
        }

        var product = await productCatalog.GetByIdAsync(request.ProductId, cancellationToken);
        if (product is null || product.IsArchived)
        {
            return Result<CartDto>.Failure(OrderErrors.ProductUnavailable(request.ProductId));
        }

        var cart = await cartRepository.GetOrCreateAsync(customerId, cancellationToken);

        try
        {
            cart.AddItem(request.ProductId, request.Quantity);
        }
        catch (InvalidOperationException ex)
        {
            return Result<CartDto>.Failure(OrderErrors.CartTooLarge(ex.Message));
        }

        await cartRepository.SaveAsync(cart, cancellationToken);

        return Result<CartDto>.Success(await cartViewBuilder.BuildAsync(cart, cancellationToken));
    }
}