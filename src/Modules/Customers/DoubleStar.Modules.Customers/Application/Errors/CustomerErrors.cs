// Application/Errors/CustomerErrors.cs
namespace DoubleStar.Modules.Customers.Application.Errors;

public static class CustomerErrors
{
    public static Error NotFound(Guid id) => new(
        "Customers.NotFound", $"Customer with ID '{id}' was not found.", ErrorType.NotFound);

    public static Error PhoneAlreadyInUse(string phone) => new(
        "Customers.PhoneAlreadyInUse", $"A customer with phone '{phone}' already exists.", ErrorType.Conflict);
}