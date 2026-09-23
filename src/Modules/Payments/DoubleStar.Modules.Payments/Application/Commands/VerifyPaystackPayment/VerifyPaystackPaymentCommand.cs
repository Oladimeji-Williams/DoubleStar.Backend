// Application/Commands/VerifyPaystackPaymentCommand/VerifyPaystackPaymentCommand.cs
using DoubleStar.Modules.Payments.Application.DTOs;

namespace DoubleStar.Modules.Payments.Application.Commands.VerifyPaystackPaymentCommand;

public sealed record VerifyPaystackPaymentCommand(string Reference) : IRequest<Result<PaymentTransactionDto>>;