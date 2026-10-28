// Called by the browser when Paystack redirects back. The webhook is the source of truth and usually
// wins the race; this just makes the UI feel instant and covers a delayed/missed webhook.
using Microsoft.Extensions.Logging;
using DoubleStar.SharedKernel.Abstractions.Authentication;
using DoubleStar.SharedKernel.Contracts.Identity;
using DoubleStar.SharedKernel.Contracts.Payments;
using DoubleStar.Modules.Orders.Application.Abstractions;
using DoubleStar.Modules.Orders.Application.DTOs;
using DoubleStar.Modules.Orders.Application.Errors;
using DoubleStar.Modules.Orders.Application.Mappings;

namespace DoubleStar.Modules.Orders.Application.Commands.VerifyOrderPayment;

public sealed record VerifyOrderPaymentCommand(int OrderId) : IRequest<Result<OrderDto>>;