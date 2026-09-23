// Application/Commands/CompleteSaleCommand/CompleteSaleCommand.cs
using DoubleStar.Modules.Sales.Application.DTOs;

namespace DoubleStar.Modules.Sales.Application.Commands.CompleteSaleCommand;

public sealed record CompleteSaleCommand(int SaleId) : IRequest<Result<SaleDto>>;