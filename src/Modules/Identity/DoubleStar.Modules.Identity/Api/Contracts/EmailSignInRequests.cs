// Identity/Api/Contracts/EmailSignInRequests.cs
namespace DoubleStar.Modules.Identity.Api.Contracts;

public sealed record RequestEmailSignInCodeRequest(string Email);
public sealed record VerifyEmailSignInCodeRequest(string Email, string Code);