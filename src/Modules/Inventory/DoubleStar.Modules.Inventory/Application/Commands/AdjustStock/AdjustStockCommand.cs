namespace DoubleStar.Modules.Inventory.Application.Commands.AdjustStockCommand;

public sealed record AdjustStockCommand(int ProductId, int NewQuantity, string Reason) : IRequest<Result>;