// Api/Contracts/RegisterCustomerRequest.cs
namespace DoubleStar.Modules.Identity.Api.Contracts;
public sealed record RegisterCustomerRequest(string Email, string Password, string? TurnstileToken);
