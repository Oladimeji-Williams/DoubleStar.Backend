// A guest fills a localStorage cart, then signs in. The frontend sends it here ONCE and we union it into
// the server cart. Lines for products that no longer exist are dropped silently — a stale guest cart
// must never block a login.
using DoubleStar.SharedKernel.Abstractions.Authentication;
using DoubleStar.SharedKernel.Contracts.Catalog;
using DoubleStar.SharedKernel.Contracts.Identity;
using DoubleStar.Modules.Orders.Application.Abstractions;
using DoubleStar.Modules.Orders.Application.DTOs;
using DoubleStar.Modules.Orders.Application.Services;
using DoubleStar.Modules.Orders.Domain.Entities;

namespace DoubleStar.Modules.Orders.Application.Commands.MergeCart;

public sealed class MergeCartCommandHandler(
    ICartRepository cartRepository,
    ICurrentUser currentUser,
    IProductCatalog productCatalog,
    CartViewBuilder cartViewBuilder)
    : IRequestHandler<MergeCartCommand, Result<CartDto>>
{
    public async Task<Result<CartDto>> Handle(MergeCartCommand request, CancellationToken cancellationToken)
    {
        if (currentUser.UserId is not Guid customerId)
        {
            return Result<CartDto>.Failure(UserErrors.NotAuthenticated());
        }

        var cart = await cartRepository.GetOrCreateAsync(customerId, cancellationToken);

        if (request.Lines.Count > 0)
        {
            var products = (await productCatalog.GetByIdsAsync(
                    request.Lines.Select(l => l.ProductId).Distinct(), cancellationToken))
                .Where(p => !p.IsArchived)
                .ToDictionary(p => p.Id);

            foreach (var line in request.Lines.Where(l => products.ContainsKey(l.ProductId)))
            {
                try
                {
                    cart.AddItem(line.ProductId, line.Quantity);
                }
                catch (InvalidOperationException)
                {
                    break; // cart full — keep what fit
                }
            }

            await cartRepository.SaveAsync(cart, cancellationToken);
        }

        return Result<CartDto>.Success(await cartViewBuilder.BuildAsync(cart, cancellationToken));
    }
}