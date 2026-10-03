// Abstractions/Authentication/LoginOutcome.cs — full replacement
namespace DoubleStar.SharedKernel.Abstractions.Authentication;

public abstract record LoginOutcome
{
    public sealed record Success(AuthenticationResult Result) : LoginOutcome;
    public sealed record InvalidCredentials : LoginOutcome;
    public sealed record AccountLockedOut(DateTimeOffset? LockoutEndUtc) : LoginOutcome;
    public sealed record AccountInactive : LoginOutcome;
    public sealed record TwoFactorRequired(string ChallengeToken) : LoginOutcome;
    public sealed record InvalidTwoFactorCode : LoginOutcome;
    public sealed record EmailNotConfirmed : LoginOutcome;
}