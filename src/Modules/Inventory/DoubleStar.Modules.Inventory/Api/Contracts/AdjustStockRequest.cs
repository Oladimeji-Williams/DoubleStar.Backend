// Api/Contracts/AdjustStockRequest.cs
namespace DoubleStar.Modules.Inventory.Api.Contracts;

public sealed record AdjustStockRequest(int ProductId, int NewQuantity, string Reason);