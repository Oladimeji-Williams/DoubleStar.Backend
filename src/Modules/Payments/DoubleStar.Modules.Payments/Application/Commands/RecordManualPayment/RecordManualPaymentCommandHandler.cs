// .../RecordManualPaymentCommandHandler.cs
using DoubleStar.Modules.Payments.Application.Abstractions;
using DoubleStar.Modules.Payments.Application.DTOs;
using DoubleStar.Modules.Payments.Application.Mappings;
using DoubleStar.Modules.Payments.Domain.Entities;

namespace DoubleStar.Modules.Payments.Application.Commands.RecordManualPaymentCommand;

public sealed class RecordManualPaymentCommandHandler(IPaymentTransactionRepository paymentTransactionRepository)
    : IRequestHandler<RecordManualPaymentCommand, Result<PaymentTransactionDto>>
{
    public async Task<Result<PaymentTransactionDto>> Handle(
        RecordManualPaymentCommand request, CancellationToken cancellationToken)
    {
        var transaction = PaymentTransaction.RecordManual(
            request.SourceType, request.SourceId, request.AmountKobo, request.Method);
        await paymentTransactionRepository.AddAsync(transaction, cancellationToken);

        return Result<PaymentTransactionDto>.Success(transaction.ToDto());
    }
}