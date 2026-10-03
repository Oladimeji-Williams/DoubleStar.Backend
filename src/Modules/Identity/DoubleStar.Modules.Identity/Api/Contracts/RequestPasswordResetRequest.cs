// Identity/Api/Contracts/UpdateProfileRequest.cs
namespace DoubleStar.Modules.Identity.Api.Contracts;

public sealed record RequestPasswordResetRequest(string Email, string? TurnstileToken);