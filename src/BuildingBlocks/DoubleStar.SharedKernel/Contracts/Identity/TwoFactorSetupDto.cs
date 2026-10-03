// SharedKernel/Contracts/Identity/TwoFactorSetupDto.cs
namespace DoubleStar.SharedKernel.Contracts.Identity;

public sealed record TwoFactorSetupDto(string SharedKey, string AuthenticatorUri);