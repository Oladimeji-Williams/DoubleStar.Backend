// Application/Errors/AuthErrors.cs
namespace DoubleStar.Modules.Identity.Application.Errors;

public static class AuthErrors
{
    public static Error EmailRequired() => new(
        "Auth.EmailRequired", "An email address is required.", ErrorType.Validation);
    public static Error EmailAlreadyExists() => new(
        "Auth.EmailAlreadyExists", "An account with this email already exists.", ErrorType.Conflict);
    public static Error InvalidCredentials() => new(
        "Auth.InvalidCredentials", "The email/phone or password is incorrect.", ErrorType.Unauthorized);
    public static Error AccountInactive() => new(
        "Auth.AccountInactive", "This account has been deactivated.", ErrorType.Forbidden);
    public static Error AccountLockedOut(DateTimeOffset? until) => new(
        "Auth.AccountLockedOut", $"Account is locked until {until?.ToString("u") ?? "unknown"}.", ErrorType.Unauthorized);
    public static Error RefreshTokenInvalid() => new(
        "Auth.RefreshTokenInvalid", "The refresh token is invalid or has expired.", ErrorType.Unauthorized);
    public static Error RegistrationFailed(string details) => new(
        "Auth.RegistrationFailed", details, ErrorType.Validation);
    public static Error ChangePasswordFailed(string details) => new(
        "Auth.ChangePasswordFailed", details, ErrorType.Validation);
    public static Error InvalidRole(string role) => new(
        "Auth.InvalidRole", $"'{role}' is not a valid staff role.", ErrorType.Validation);
    public static Error CannotModifyOwnAccount() => new(
        "Auth.CannotModifyOwnAccount",
        "You can't change your own role or active status here — use your profile page for your own details.",
        ErrorType.Forbidden);
    public static Error TwoFactorRequired() => new(
        "Auth.TwoFactorRequired", "Two-factor authentication code required.", ErrorType.Unauthorized);
    public static Error InvalidTwoFactorCode() => new(
        "Auth.InvalidTwoFactorCode", "That code is incorrect or has expired.", ErrorType.Validation);
    public static Error EmailNotConfirmed() => new(
        "Auth.EmailNotConfirmed", "Please confirm your email address before signing in.", ErrorType.Unauthorized);
}