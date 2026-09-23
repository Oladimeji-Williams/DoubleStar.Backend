// Application/Commands/CreateSaleCommand/CreateSaleCommand.cs
using DoubleStar.Modules.Sales.Application.DTOs;

namespace DoubleStar.Modules.Sales.Application.Commands.CreateSaleCommand;

/// <summary>
/// Exactly one of CustomerId (an existing account/known walk-in) or
/// WalkInName (a brand-new walk-in) is expected; leaving both null records
/// a fully anonymous cash sale with no customer attached at all.
/// </summary>
public sealed record CreateSaleCommand(Guid? CustomerId, string? WalkInName, string? WalkInPhone)
    : IRequest<Result<SaleDto>>;