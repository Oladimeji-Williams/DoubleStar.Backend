// Identity/Api/Contracts/UpdateProfileRequest.cs
namespace DoubleStar.Modules.Identity.Api.Contracts;

public sealed record ConfirmPasswordResetRequest(string Email, string Token, string NewPassword);