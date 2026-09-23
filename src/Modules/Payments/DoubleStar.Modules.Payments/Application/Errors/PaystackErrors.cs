// Application/Errors/PaymentErrors.cs
namespace DoubleStar.Modules.Payments.Application.Errors;

public static class PaymentErrors
{
    public static Error NotFound(Guid id) => new("Payments.NotFound", $"Payment '{id}' was not found.", ErrorType.NotFound);
    public static Error ReferenceNotFound(string reference) => new(
        "Payments.ReferenceNotFound", $"No payment found for reference '{reference}'.", ErrorType.NotFound);
    public static Error GatewayFailure(string details) => new("Payments.GatewayFailure", details, ErrorType.Failure);
    public static Error InvalidState(string message) => new("Payments.InvalidState", message, ErrorType.Conflict);
}