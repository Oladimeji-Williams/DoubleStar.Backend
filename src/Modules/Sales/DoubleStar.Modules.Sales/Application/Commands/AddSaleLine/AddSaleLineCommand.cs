// Application/Commands/AddSaleLineCommand/AddSaleLineCommand.cs
using DoubleStar.Modules.Sales.Application.DTOs;

namespace DoubleStar.Modules.Sales.Application.Commands.AddSaleLineCommand;

public sealed record AddSaleLineCommand(int SaleId, int ProductId, int? Quantity, string? SerialNumber)
    : IRequest<Result<SaleLineDto>>;