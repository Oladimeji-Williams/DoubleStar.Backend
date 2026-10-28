// Run every minute by ExpireUnpaidOrdersService. Frees stock held by orders nobody paid for.
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using DoubleStar.SharedKernel.Contracts.Orders;
using DoubleStar.SharedKernel.Contracts.Payments;
using DoubleStar.Modules.Orders.Application.Abstractions;
using DoubleStar.Modules.Orders.Application.Mappings;
using DoubleStar.Modules.Orders.Application.Options;
using DoubleStar.Modules.Orders.Application.Services;

namespace DoubleStar.Modules.Orders.Application.Commands.ExpireUnpaidOrders;

/// <summary>Returns how many orders were expired.</summary>
public sealed record ExpireUnpaidOrdersCommand : IRequest<Result<int>>;