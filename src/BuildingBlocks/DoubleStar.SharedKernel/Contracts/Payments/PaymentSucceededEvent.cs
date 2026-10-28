using MediatR;

namespace DoubleStar.SharedKernel.Contracts.Payments;

public sealed record PaymentSucceededEvent(
    Guid PaymentId,
    PaymentSourceType SourceType,
    int SourceId,
    long AmountKobo,
    string Reference) : INotification;
