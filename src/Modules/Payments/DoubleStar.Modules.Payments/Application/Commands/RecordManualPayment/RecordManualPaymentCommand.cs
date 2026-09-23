// Application/Commands/RecordManualPaymentCommand/RecordManualPaymentCommand.cs
using DoubleStar.SharedKernel.Contracts.Payments;
using DoubleStar.Modules.Payments.Application.DTOs;
using DoubleStar.Modules.Payments.Domain.Enums;

namespace DoubleStar.Modules.Payments.Application.Commands.RecordManualPaymentCommand;

public sealed record RecordManualPaymentCommand(
    PaymentSourceType SourceType, int SourceId, long AmountKobo, PaymentMethod Method)
    : IRequest<Result<PaymentTransactionDto>>;