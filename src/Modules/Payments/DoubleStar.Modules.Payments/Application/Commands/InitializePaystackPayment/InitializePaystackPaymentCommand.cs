// Application/Commands/InitializePaystackPaymentCommand/InitializePaystackPaymentCommand.cs
using DoubleStar.SharedKernel.Contracts.Payments;

namespace DoubleStar.Modules.Payments.Application.Commands.InitializePaystackPaymentCommand;

public sealed record InitializePaystackPaymentCommand(
    PaymentSourceType SourceType, int SourceId, long AmountKobo, string Email, string CallbackUrl)
    : IRequest<Result<string>>; // returns the Paystack authorization URL to redirect to