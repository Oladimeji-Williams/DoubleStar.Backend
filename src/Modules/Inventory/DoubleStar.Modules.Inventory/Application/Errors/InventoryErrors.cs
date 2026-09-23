// Application/Errors/InventoryErrors.cs
namespace DoubleStar.Modules.Inventory.Application.Errors;

public static class InventoryErrors
{
    public static Error ProductNotTracked(int productId) => new(
        "Inventory.ProductNotTracked", $"Product '{productId}' has no stock record yet.", ErrorType.NotFound);

    public static Error InsufficientStock(int productId) => new(
        "Inventory.InsufficientStock", $"Not enough stock for product '{productId}'.", ErrorType.Conflict);

    public static Error SerialAlreadyExists(string serial) => new(
        "Inventory.SerialAlreadyExists", $"Serial '{serial}' is already in inventory.", ErrorType.Conflict);

    public static Error SerialNotFound(string serial) => new(
        "Inventory.SerialNotFound", $"No unit found with serial '{serial}'.", ErrorType.NotFound);

    public static Error SerialAlreadySold(string serial) => new(
        "Inventory.SerialAlreadySold", $"Unit '{serial}' has already been sold.", ErrorType.Conflict);

    public static Error NotBulkTracked(int productId) => new(
        "Inventory.NotBulkTracked",
        $"Product '{productId}' is serialized — reserve/release by serial number, not quantity.",
        ErrorType.Validation);
}