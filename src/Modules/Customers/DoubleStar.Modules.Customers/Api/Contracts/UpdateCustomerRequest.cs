// Api/Contracts/UpdateCustomerRequest.cs
namespace DoubleStar.Modules.Customers.Api.Contracts;

public sealed record UpdateCustomerRequest(string Name, string? Phone, string? Email, string? Address);