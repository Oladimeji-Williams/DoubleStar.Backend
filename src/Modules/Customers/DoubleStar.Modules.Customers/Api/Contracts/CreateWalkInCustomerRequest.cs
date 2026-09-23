// Api/Contracts/CreateWalkInCustomerRequest.cs
namespace DoubleStar.Modules.Customers.Api.Contracts;

public sealed record CreateWalkInCustomerRequest(string Name, string? Phone, string? Email);