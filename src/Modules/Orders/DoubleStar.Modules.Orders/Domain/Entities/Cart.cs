using DoubleStar.SharedKernel.Domain;

namespace DoubleStar.Modules.Orders.Domain.Entities;

/// <summary>
/// A customer's server-side cart. It stores intent only (product + quantity) — never a price.
/// Prices and stock are read live from Catalog/Inventory, then snapshotted into the Order at checkout.
/// </summary>
public sealed class Cart : Entity
{
    public const int MaxQuantityPerLine = 20;
    public const int MaxDistinctItems = 50;

    private readonly List<CartItem> _items = [];

    public Guid CustomerId { get; private set; }
    public IReadOnlyList<CartItem> Items => _items;

    private Cart() { }

    public static Cart CreateFor(Guid customerId) => new() { CustomerId = customerId };

    /// <summary>Adds to the existing quantity, clamped to MaxQuantityPerLine.</summary>
    public void AddItem(int productId, int quantity)
    {
        if (quantity <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(quantity), "Quantity must be greater than zero.");
        }

        var existing = _items.FirstOrDefault(i => i.ProductId == productId);

        if (existing is not null)
        {
            existing.SetQuantity(Math.Min(existing.Quantity + quantity, MaxQuantityPerLine));
            return;
        }

        if (_items.Count >= MaxDistinctItems)
        {
            throw new InvalidOperationException($"A cart can hold at most {MaxDistinctItems} different products.");
        }

        _items.Add(CartItem.Create(productId, Math.Min(quantity, MaxQuantityPerLine)));
    }

    /// <summary>Sets an absolute quantity. Zero (or less) removes the line.</summary>
    public void SetQuantity(int productId, int quantity)
    {
        if (quantity <= 0)
        {
            RemoveItem(productId);
            return;
        }

        var existing = _items.FirstOrDefault(i => i.ProductId == productId);

        if (existing is null)
        {
            AddItem(productId, quantity);
            return;
        }

        existing.SetQuantity(Math.Min(quantity, MaxQuantityPerLine));
    }

    public void RemoveItem(int productId) => _items.RemoveAll(i => i.ProductId == productId);

    public void Clear() => _items.Clear();
}