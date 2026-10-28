using DoubleStar.SharedKernel.Abstractions.Authentication;
using DoubleStar.SharedKernel.Contracts.Identity;
using DoubleStar.SharedKernel.Contracts.Orders;
using DoubleStar.SharedKernel.Contracts.Payments;
using DoubleStar.Modules.Orders.Application.Abstractions;
using DoubleStar.Modules.Orders.Application.Errors;
using DoubleStar.Modules.Orders.Application.Mappings;
using DoubleStar.Modules.Orders.Application.Services;

namespace DoubleStar.Modules.Orders.Application.Commands.CancelOrder;

public sealed record CancelOrderCommand(int OrderId) : IRequest<Result>;
