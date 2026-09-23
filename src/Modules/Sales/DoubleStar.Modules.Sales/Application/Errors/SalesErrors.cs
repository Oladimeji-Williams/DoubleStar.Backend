// Application/Errors/SalesErrors.cs
namespace DoubleStar.Modules.Sales.Application.Errors;

public static class SalesErrors
{
    public static Error NotFound(int id) => new("Sales.NotFound", $"Sale '{id}' was not found.", ErrorType.NotFound);
    public static Error NotDraft(int id) => new("Sales.NotDraft", $"Sale '{id}' is not in Draft status.", ErrorType.Conflict);
    public static Error NoLines(int id) => new("Sales.NoLines", $"Sale '{id}' has no line items.", ErrorType.Validation);
    public static Error ProductNotFound(int productId) => new(
        "Sales.ProductNotFound", $"Product '{productId}' was not found.", ErrorType.NotFound);
    public static Error SerialRequired() => new(
        "Sales.SerialRequired", "A serial number is required for a serialized product.", ErrorType.Validation);
    public static Error QuantityNotAllowed() => new(
        "Sales.QuantityNotAllowed",
        "Quantity does not apply to a serialized product — provide a serial number instead.",
        ErrorType.Validation);
    public static Error SerialNotAvailable(string serial) => new(
        "Sales.SerialNotAvailable", $"Unit '{serial}' is not currently in stock.", ErrorType.Conflict);
}