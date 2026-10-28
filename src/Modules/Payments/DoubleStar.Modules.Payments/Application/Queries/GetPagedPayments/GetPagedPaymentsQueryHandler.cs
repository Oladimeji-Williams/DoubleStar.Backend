using DoubleStar.BuildingBlocks.Application.Common.Queries.Pagination;
using DoubleStar.Modules.Payments.Application.Abstractions;
using DoubleStar.Modules.Payments.Application.DTOs;

namespace DoubleStar.Modules.Payments.Application.Queries.GetPagedPayments;

public sealed class GetPagedPaymentsQueryHandler(
    IPaymentTransactionRepository paymentTransactionRepository)
    : IRequestHandler<
        GetPagedPaymentsQuery,
        Result<PagedResult<PaymentListDto>>>
{
    public async Task<Result<PagedResult<PaymentListDto>>> Handle(
        GetPagedPaymentsQuery request,
        CancellationToken cancellationToken)
    {
        var result =
            await paymentTransactionRepository.GetPagedAsync(
                request.Query,
                cancellationToken);

        var dto =
            new PagedResult<PaymentListDto>(
                result.Items
                    .Select(payment =>
                        new PaymentListDto(
                            payment.Id,
                            payment.SourceType,
                            payment.SourceId,
                            payment.AmountKobo,
                            payment.TotalRefundedKobo,
                            payment.Method.ToString(),
                            payment.Status.ToString(),
                            payment.PaystackReference,
                            payment.CreatedAt))
                    .ToList(),
                result.Page,
                result.PageSize,
                result.TotalCount);

        return Result<PagedResult<PaymentListDto>>.Success(dto);
    }
}