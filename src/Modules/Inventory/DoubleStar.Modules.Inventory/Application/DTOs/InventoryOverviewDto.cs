namespace DoubleStar.Modules.Inventory.Application.DTOs;

public sealed record InventoryOverviewDto(
    int ProductId,
    string ProductName,
    string Sku,
    string TrackingMode,
    int AvailableQuantity,
    int ReservedQuantity,
    string Status);