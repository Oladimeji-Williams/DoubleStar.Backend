// A guest fills a localStorage cart, then signs in. The frontend sends it here ONCE and we union it into
// the server cart. Lines for products that no longer exist are dropped silently — a stale guest cart
// must never block a login.
using DoubleStar.SharedKernel.Abstractions.Authentication;
using DoubleStar.SharedKernel.Contracts.Catalog;
using DoubleStar.SharedKernel.Contracts.Identity;
using DoubleStar.Modules.Orders.Application.Abstractions;
using DoubleStar.Modules.Orders.Application.DTOs;
using DoubleStar.Modules.Orders.Application.Services;
using DoubleStar.Modules.Orders.Domain.Entities;

namespace DoubleStar.Modules.Orders.Application.Commands.MergeCart;

public sealed record MergeCartLine(int ProductId, int Quantity);

public sealed record MergeCartCommand(IReadOnlyList<MergeCartLine> Lines) : IRequest<Result<CartDto>>;
