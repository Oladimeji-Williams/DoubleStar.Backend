using DoubleStar.SharedKernel.Abstractions.Authentication;
using DoubleStar.SharedKernel.Contracts.Catalog;
using DoubleStar.SharedKernel.Contracts.Identity;
using DoubleStar.Modules.Orders.Application.Abstractions;
using DoubleStar.Modules.Orders.Application.DTOs;
using DoubleStar.Modules.Orders.Application.Errors;
using DoubleStar.Modules.Orders.Application.Services;
using DoubleStar.Modules.Orders.Domain.Entities;

namespace DoubleStar.Modules.Orders.Application.Commands.AddCartItem;

public sealed record AddCartItemCommand(int ProductId, int Quantity) : IRequest<Result<CartDto>>;