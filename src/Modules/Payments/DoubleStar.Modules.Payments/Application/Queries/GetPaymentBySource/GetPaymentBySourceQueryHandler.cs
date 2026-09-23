// .../GetPaymentsBySourceQueryHandler.cs
using DoubleStar.Modules.Payments.Application.Abstractions;
using DoubleStar.Modules.Payments.Application.DTOs;
using DoubleStar.Modules.Payments.Application.Mappings;

namespace DoubleStar.Modules.Payments.Application.Queries.GetPaymentsBySourceQuery;

public sealed class GetPaymentsBySourceQueryHandler(IPaymentTransactionRepository paymentTransactionRepository)
    : IRequestHandler<GetPaymentsBySourceQuery, Result<IReadOnlyList<PaymentTransactionDto>>>
{
    public async Task<Result<IReadOnlyList<PaymentTransactionDto>>> Handle(
        GetPaymentsBySourceQuery request, CancellationToken cancellationToken)
    {
        var transactions = await paymentTransactionRepository.GetForSourceAsync(
            request.SourceType, request.SourceId, cancellationToken);
        return Result<IReadOnlyList<PaymentTransactionDto>>.Success(transactions.Select(t => t.ToDto()).ToList());
    }
}