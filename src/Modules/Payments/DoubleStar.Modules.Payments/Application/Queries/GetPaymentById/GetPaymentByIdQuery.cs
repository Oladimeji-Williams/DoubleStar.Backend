using DoubleStar.Modules.Payments.Application.DTOs;

namespace DoubleStar.Modules.Payments.Application.Queries.GetPaymentById;

public sealed record GetPaymentByIdQuery(Guid Id) : IRequest<Result<PaymentTransactionDto>>;