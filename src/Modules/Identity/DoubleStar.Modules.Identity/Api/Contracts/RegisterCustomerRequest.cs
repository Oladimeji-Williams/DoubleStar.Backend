// Api/Contracts/RegisterCustomerRequest.cs
namespace DoubleStar.Modules.Identity.Api.Contracts;

public sealed record RegisterCustomerRequest(string FirstName, string LastName, string Email, string? Phone, string Password);