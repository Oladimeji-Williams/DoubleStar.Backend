// Api/Contracts/ChangePasswordRequest.cs
namespace DoubleStar.Modules.Identity.Api.Contracts;

public sealed record ChangePasswordRequest(string CurrentPassword, string NewPassword);