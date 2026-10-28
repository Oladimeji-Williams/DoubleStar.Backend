using DoubleStar.BuildingBlocks.Application.Common;
using DoubleStar.Modules.Reporting.Application.DTOs;
using DoubleStar.SharedKernel.Contracts.Payments;

namespace DoubleStar.Modules.Reporting.Application.Queries.GetPaymentMethodSummaryQuery;

public sealed class GetPaymentMethodSummaryQueryHandler(
    IPaymentAnalyticsReader paymentAnalyticsReader)
    : IRequestHandler<
        GetPaymentMethodSummaryQuery,
        Result<IReadOnlyList<PaymentMethodReportDto>>>
{
    public async Task<Result<IReadOnlyList<PaymentMethodReportDto>>> Handle(
        GetPaymentMethodSummaryQuery request,
        CancellationToken cancellationToken)
    {
        var payments =
            await paymentAnalyticsReader.GetPaymentMethodSummaryAsync(
                request.FromUtc,
                request.ToUtc,
                cancellationToken);

        var result = payments
            .Select(payment => new PaymentMethodReportDto(
                payment.PaymentMethod,
                payment.TransactionCount,
                payment.AmountKobo))
            .ToList();

        return Result<IReadOnlyList<PaymentMethodReportDto>>
            .Success(result);
    }
}