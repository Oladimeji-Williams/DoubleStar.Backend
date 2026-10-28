namespace DoubleStar.SharedKernel.Contracts.Payments;

public sealed record PaymentInitiationDto(Guid PaymentId, string AuthorizationUrl, string Reference);
