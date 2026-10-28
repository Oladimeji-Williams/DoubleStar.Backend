using DoubleStar.SharedKernel.Abstractions.Authentication;
using DoubleStar.SharedKernel.Contracts.Identity;
using DoubleStar.Modules.Orders.Application.Abstractions;
using DoubleStar.Modules.Orders.Application.DTOs;
using DoubleStar.Modules.Orders.Application.Services;

namespace DoubleStar.Modules.Orders.Application.Queries.GetMyCart;
public sealed class GetMyCartQueryHandler(
    ICartRepository cartRepository,
    ICurrentUser currentUser,
    CartViewBuilder cartViewBuilder)
    : IRequestHandler<GetMyCartQuery, Result<CartDto>>
{
    public async Task<Result<CartDto>> Handle(GetMyCartQuery request, CancellationToken cancellationToken)
    {
        if (currentUser.UserId is not Guid customerId)
        {
            return Result<CartDto>.Failure(UserErrors.NotAuthenticated());
        }

        var cart = await cartRepository.GetByCustomerIdAsync(customerId, cancellationToken);

        return Result<CartDto>.Success(
            cart is null ? CartViewBuilder.Empty : await cartViewBuilder.BuildAsync(cart, cancellationToken));
    }
}