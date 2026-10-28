using DoubleStar.SharedKernel.Domain;

namespace DoubleStar.Modules.Orders.Domain.Entities;

public sealed class CartItem : Entity
{
    public int CartId { get; private set; }
    public int ProductId { get; private set; }
    public int Quantity { get; private set; }

    private CartItem() { }

    internal static CartItem Create(int productId, int quantity) => new()
    {
        ProductId = productId,
        Quantity = quantity,
    };

    internal void SetQuantity(int quantity) => Quantity = quantity;
}