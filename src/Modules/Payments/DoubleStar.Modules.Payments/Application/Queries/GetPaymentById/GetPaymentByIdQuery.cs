// Application/Queries/GetPaymentByIdQuery/GetPaymentByIdQuery.cs
using DoubleStar.Modules.Payments.Application.DTOs;

namespace DoubleStar.Modules.Payments.Application.Queries.GetPaymentByIdQuery;

public sealed record GetPaymentByIdQuery(Guid Id) : IRequest<Result<PaymentTransactionDto>>;