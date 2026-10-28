namespace DoubleStar.Modules.Payments.Application.Commands.RefundPayment;

public sealed record RefundPaymentCommand(Guid PaymentId, long AmountKobo, string Reason) : IRequest<Result>;