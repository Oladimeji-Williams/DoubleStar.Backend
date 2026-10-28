using DoubleStar.SharedKernel.Abstractions.Authentication;
using DoubleStar.SharedKernel.Contracts.Identity;
using DoubleStar.Modules.Orders.Application.Abstractions;
using DoubleStar.Modules.Orders.Application.DTOs;
using DoubleStar.Modules.Orders.Application.Services;

namespace DoubleStar.Modules.Orders.Application.Commands.ClearCart;

public sealed class ClearCartCommandHandler(ICartRepository cartRepository, ICurrentUser currentUser)
    : IRequestHandler<ClearCartCommand, Result<CartDto>>
{
    public async Task<Result<CartDto>> Handle(ClearCartCommand request, CancellationToken cancellationToken)
    {
        if (currentUser.UserId is not Guid customerId)
        {
            return Result<CartDto>.Failure(UserErrors.NotAuthenticated());
        }

        var cart = await cartRepository.GetByCustomerIdAsync(customerId, cancellationToken);
        if (cart is not null)
        {
            cart.Clear();
            await cartRepository.SaveAsync(cart, cancellationToken);
        }

        return Result<CartDto>.Success(CartViewBuilder.Empty);
    }
}