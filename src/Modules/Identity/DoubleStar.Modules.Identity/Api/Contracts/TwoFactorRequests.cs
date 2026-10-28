namespace DoubleStar.Modules.Identity.Api.Contracts;

public sealed record ConfirmTwoFactorRequest(string Code);
public sealed record DisableTwoFactorRequest(string CurrentPassword);
public sealed record VerifyTwoFactorChallengeRequest(string ChallengeToken, string Code);