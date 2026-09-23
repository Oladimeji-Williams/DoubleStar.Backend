// Api/Contracts/LoginRequest.cs
namespace DoubleStar.Modules.Identity.Api.Contracts;

public sealed record LoginRequest(string EmailOrPhone, string Password);