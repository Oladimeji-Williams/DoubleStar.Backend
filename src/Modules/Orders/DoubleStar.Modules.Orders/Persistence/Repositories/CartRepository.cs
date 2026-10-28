using Microsoft.EntityFrameworkCore;
using DoubleStar.Modules.Orders.Application.Abstractions;
using DoubleStar.Modules.Orders.Domain.Entities;

namespace DoubleStar.Modules.Orders.Persistence.Repositories;

public sealed class CartRepository(OrdersDbContext dbContext) : ICartRepository
{
    public Task<Cart?> GetByCustomerIdAsync(Guid customerId, CancellationToken cancellationToken = default) =>
        dbContext.Carts
            .Include(c => c.Items)
            .FirstOrDefaultAsync(c => c.CustomerId == customerId, cancellationToken);

    public async Task<Cart> GetOrCreateAsync(Guid customerId, CancellationToken cancellationToken = default)
    {
        var cart = await GetByCustomerIdAsync(customerId, cancellationToken);
        if (cart is not null)
        {
            return cart;
        }

        cart = Cart.CreateFor(customerId);
        await dbContext.Carts.AddAsync(cart, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);

        return cart;
    }

    // Carts and their items are always loaded tracked, so SaveChanges is enough
    // (it picks up added, changed AND removed items).
    public Task SaveAsync(Cart cart, CancellationToken cancellationToken = default) =>
        dbContext.SaveChangesAsync(cancellationToken);
}
