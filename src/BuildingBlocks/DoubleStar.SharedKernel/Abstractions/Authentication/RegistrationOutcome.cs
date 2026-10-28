namespace DoubleStar.SharedKernel.Contracts.Identity;

public sealed record RegistrationOutcome(
    Guid UserId,
    string Email,
    bool EmailSent,
    string Message);