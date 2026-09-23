// Application/Queries/GetPaymentsBySourceQuery/GetPaymentsBySourceQuery.cs
using DoubleStar.SharedKernel.Contracts.Payments;
using DoubleStar.Modules.Payments.Application.DTOs;

namespace DoubleStar.Modules.Payments.Application.Queries.GetPaymentsBySourceQuery;

public sealed record GetPaymentsBySourceQuery(PaymentSourceType SourceType, int SourceId)
    : IRequest<Result<IReadOnlyList<PaymentTransactionDto>>>;