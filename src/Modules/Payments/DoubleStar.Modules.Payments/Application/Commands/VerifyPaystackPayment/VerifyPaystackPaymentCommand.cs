using DoubleStar.Modules.Payments.Application.DTOs;

namespace DoubleStar.Modules.Payments.Application.Commands.VerifyPaystackPayment;

public sealed record VerifyPaystackPaymentCommand(string Reference) : IRequest<Result<PaymentTransactionDto>>;