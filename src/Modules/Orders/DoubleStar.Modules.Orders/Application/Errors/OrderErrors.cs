namespace DoubleStar.Modules.Orders.Application.Errors;

public static class OrderErrors
{
    public static Error NotFound(int id) => new("Orders.NotFound", $"Order '{id}' was not found.", ErrorType.NotFound);

    public static Error ProductUnavailable(int productId) => new(
        "Orders.ProductUnavailable", $"Product '{productId}' is no longer available.", ErrorType.Conflict);

    public static Error CartEmpty() => new("Orders.CartEmpty", "Your cart is empty.", ErrorType.Validation);

    public static Error CartHasIssues() => new(
        "Orders.CartHasIssues", "Some items in your cart are unavailable. Review your cart and try again.", ErrorType.Conflict);

    public static Error OutOfStock(string productName) => new(
        "Orders.OutOfStock", $"'{productName}' doesn't have enough stock for the quantity in your cart.", ErrorType.Conflict);

    public static Error InvalidTransition(string message) => new("Orders.InvalidTransition", message, ErrorType.Conflict);

    public static Error NotPayable() => new(
        "Orders.NotPayable", "This order is no longer awaiting payment.", ErrorType.Conflict);

    public static Error PaymentWindowElapsed() => new(
        "Orders.PaymentWindowElapsed", "The payment window for this order has elapsed.", ErrorType.Conflict);

    public static Error PaymentNotStarted() => new(
        "Orders.PaymentNotStarted", "No payment has been started for this order yet.", ErrorType.Conflict);

    public static Error EmailRequired() => new(
        "Orders.EmailRequired", "Your account needs an email address before you can pay online.", ErrorType.Validation);

    public static Error CartTooLarge(string message) => new("Orders.CartTooLarge", message, ErrorType.Validation);
}