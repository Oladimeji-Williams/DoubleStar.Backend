// Application/Commands/RefundPaymentCommand/RefundPaymentCommand.cs
namespace DoubleStar.Modules.Payments.Application.Commands.RefundPaymentCommand;

public sealed record RefundPaymentCommand(Guid PaymentId, long AmountKobo, string Reason) : IRequest<Result>;