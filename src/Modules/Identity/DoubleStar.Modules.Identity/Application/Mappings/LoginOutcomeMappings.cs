// Identity/Application/Mappings/LoginOutcomeMapper.cs
using DoubleStar.SharedKernel.Abstractions.Authentication;
using DoubleStar.Modules.Identity.Application.Errors;

namespace DoubleStar.Modules.Identity.Application.Mappings;

public static class LoginOutcomeMapper
{
    public static Result<AuthenticationResult> ToResult(this LoginOutcome outcome) => outcome switch
    {
        LoginOutcome.Success(var result) => Result<AuthenticationResult>.Success(result),
        LoginOutcome.AccountLockedOut(var lockoutEnd) => Result<AuthenticationResult>.Failure(AuthErrors.AccountLockedOut(lockoutEnd)),
        LoginOutcome.InvalidCredentials => Result<AuthenticationResult>.Failure(AuthErrors.InvalidCredentials()),
        LoginOutcome.AccountInactive => Result<AuthenticationResult>.Failure(AuthErrors.AccountInactive()),
        LoginOutcome.InvalidTwoFactorCode => Result<AuthenticationResult>.Failure(AuthErrors.InvalidTwoFactorCode()),
        LoginOutcome.TwoFactorRequired => Result<AuthenticationResult>.Failure(AuthErrors.TwoFactorRequired()),
        _ => Result<AuthenticationResult>.Failure(AuthErrors.InvalidCredentials()),
    };
}