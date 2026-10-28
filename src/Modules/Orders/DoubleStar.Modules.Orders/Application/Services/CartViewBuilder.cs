// Turns the stored intent (product + qty) into what the customer sees: live price, live stock, issues.
using DoubleStar.SharedKernel.Contracts.Catalog;
using DoubleStar.SharedKernel.Contracts.Inventory;
using DoubleStar.Modules.Orders.Application.DTOs;
using DoubleStar.Modules.Orders.Domain.Entities;

namespace DoubleStar.Modules.Orders.Application.Services;

public sealed class CartViewBuilder(IProductCatalog productCatalog, IStockLevelReader stockLevelReader)
{
    public static CartDto Empty { get; } = new([], 0, 0, false);

    public async Task<CartDto> BuildAsync(Cart cart, CancellationToken cancellationToken)
    {
        if (cart.Items.Count == 0)
        {
            return Empty;
        }

        var products = (await productCatalog.GetByIdsAsync(cart.Items.Select(i => i.ProductId), cancellationToken))
            .ToDictionary(p => p.Id);

        var serializedCounts = await stockLevelReader.GetSerializedInStockCountsAsync(cancellationToken);
        var items = new List<CartItemDto>(cart.Items.Count);

        foreach (var item in cart.Items.OrderBy(i => i.Id))
        {
            if (!products.TryGetValue(item.ProductId, out var product) || product.IsArchived)
            {
                items.Add(new CartItemDto(
                    item.ProductId, "Unavailable product", string.Empty, null, 0, item.Quantity, 0,
                    AvailableQuantity: 0, IsAvailable: false, Issue: "This product is no longer available."));
                continue;
            }

            int available;
            if (product.TrackingMode == StockTrackingMode.Serialized)
            {
                available = serializedCounts.GetValueOrDefault(product.Id);
            }
            else
            {
                var level = await stockLevelReader.GetAvailableAsync(product.Id, cancellationToken);
                available = Math.Max(0, level?.AvailableQuantity ?? 0);
            }

            var issue = available <= 0
                ? "Out of stock."
                : item.Quantity > available ? $"Only {available} left in stock." : null;

            items.Add(new CartItemDto(
                product.Id,
                product.Name,
                product.Sku,
                product.ImageUrl,
                product.UnitPriceKobo,
                item.Quantity,
                product.UnitPriceKobo * item.Quantity,
                available,
                IsAvailable: issue is null,
                issue));
        }

        return new CartDto(
            items,
            items.Sum(i => i.Quantity),
            items.Sum(i => i.LineTotalKobo),
            items.Any(i => !i.IsAvailable));
    }
}