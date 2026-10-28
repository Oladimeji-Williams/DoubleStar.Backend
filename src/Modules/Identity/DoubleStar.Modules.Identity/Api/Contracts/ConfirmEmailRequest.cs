namespace DoubleStar.Modules.Identity.Api.Contracts;

public sealed record ConfirmEmailRequest(string Email, string Token);