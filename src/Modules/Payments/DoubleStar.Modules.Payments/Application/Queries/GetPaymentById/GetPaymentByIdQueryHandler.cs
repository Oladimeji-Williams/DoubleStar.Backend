// .../GetPaymentByIdQueryHandler.cs
using DoubleStar.Modules.Payments.Application.Abstractions;
using DoubleStar.Modules.Payments.Application.DTOs;
using DoubleStar.Modules.Payments.Application.Errors;
using DoubleStar.Modules.Payments.Application.Mappings;

namespace DoubleStar.Modules.Payments.Application.Queries.GetPaymentByIdQuery;

public sealed class GetPaymentByIdQueryHandler(IPaymentTransactionRepository paymentTransactionRepository)
    : IRequestHandler<GetPaymentByIdQuery, Result<PaymentTransactionDto>>
{
    public async Task<Result<PaymentTransactionDto>> Handle(GetPaymentByIdQuery request, CancellationToken cancellationToken)
    {
        var transaction = await paymentTransactionRepository.GetByIdAsync(request.Id, cancellationToken);
        return transaction is null
            ? Result<PaymentTransactionDto>.Failure(PaymentErrors.NotFound(request.Id))
            : Result<PaymentTransactionDto>.Success(transaction.ToDto());
    }
}