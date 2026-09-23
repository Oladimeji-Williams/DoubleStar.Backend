// Contracts/Inventory/StockLevelDto.cs
namespace DoubleStar.SharedKernel.Contracts.Inventory;

public sealed record StockLevelDto(int ProductId, int AvailableQuantity, int ReservedQuantity);
public sealed record SerializedUnitDto(Guid Id, int ProductId, string SerialNumber, string Status);