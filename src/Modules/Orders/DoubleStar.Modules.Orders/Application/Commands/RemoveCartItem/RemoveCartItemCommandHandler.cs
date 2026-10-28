using DoubleStar.SharedKernel.Abstractions.Authentication;
using DoubleStar.SharedKernel.Contracts.Identity;
using DoubleStar.Modules.Orders.Application.Abstractions;
using DoubleStar.Modules.Orders.Application.DTOs;
using DoubleStar.Modules.Orders.Application.Services;

namespace DoubleStar.Modules.Orders.Application.Commands.RemoveCartItem;
public sealed class RemoveCartItemCommandHandler(
    ICartRepository cartRepository,
    ICurrentUser currentUser,
    CartViewBuilder cartViewBuilder)
    : IRequestHandler<RemoveCartItemCommand, Result<CartDto>>
{
    public async Task<Result<CartDto>> Handle(RemoveCartItemCommand request, CancellationToken cancellationToken)
    {
        if (currentUser.UserId is not Guid customerId)
        {
            return Result<CartDto>.Failure(UserErrors.NotAuthenticated());
        }

        var cart = await cartRepository.GetOrCreateAsync(customerId, cancellationToken);
        cart.RemoveItem(request.ProductId);
        await cartRepository.SaveAsync(cart, cancellationToken);

        return Result<CartDto>.Success(await cartViewBuilder.BuildAsync(cart, cancellationToken));
    }
}