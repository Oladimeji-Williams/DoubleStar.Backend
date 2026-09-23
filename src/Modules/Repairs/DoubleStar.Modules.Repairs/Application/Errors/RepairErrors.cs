// Application/Errors/RepairErrors.cs
using DoubleStar.SharedKernel.Contracts.Repairs;

namespace DoubleStar.Modules.Repairs.Application.Errors;

public static class RepairErrors
{
    public static Error NotFound(int id) => new("Repairs.NotFound", $"Repair ticket '{id}' was not found.", ErrorType.NotFound);
    public static Error ProductNotFound(int productId) => new(
        "Repairs.ProductNotFound", $"Product '{productId}' was not found.", ErrorType.NotFound);
    public static Error InvalidTransition(string message) => new("Repairs.InvalidTransition", message, ErrorType.Conflict);
}