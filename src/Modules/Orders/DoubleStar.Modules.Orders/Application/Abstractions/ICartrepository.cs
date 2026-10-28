using DoubleStar.Modules.Orders.Domain.Entities;

namespace DoubleStar.Modules.Orders.Application.Abstractions;

public interface ICartRepository
{
    /// <summary>Tracked cart with items; created (and saved empty) on first use.</summary>
    Task<Cart> GetOrCreateAsync(Guid customerId, CancellationToken cancellationToken = default);

    Task<Cart?> GetByCustomerIdAsync(Guid customerId, CancellationToken cancellationToken = default);

    Task SaveAsync(Cart cart, CancellationToken cancellationToken = default);
}