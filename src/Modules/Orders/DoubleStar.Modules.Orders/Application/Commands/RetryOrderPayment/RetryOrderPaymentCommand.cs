// "Complete payment" on an order that is still PendingPayment (closed the Paystack tab, card declined, ...).
// A fresh Paystack reference is created per attempt; the window is extended so a retry isn't instantly expired.
using Microsoft.Extensions.Options;
using DoubleStar.SharedKernel.Abstractions.Authentication;
using DoubleStar.SharedKernel.Common.Options;
using DoubleStar.SharedKernel.Contracts.Identity;
using DoubleStar.SharedKernel.Contracts.Payments;
using DoubleStar.Modules.Orders.Application.Abstractions;
using DoubleStar.Modules.Orders.Application.DTOs;
using DoubleStar.Modules.Orders.Application.Errors;
using DoubleStar.Modules.Orders.Application.Options;

namespace DoubleStar.Modules.Orders.Application.Commands.RetryOrderPayment;

public sealed record RetryOrderPaymentCommand(int OrderId) : IRequest<Result<PaymentInitiationResultDto>>;