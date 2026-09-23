// Application/Commands/VoidSaleCommand/VoidSaleCommand.cs
namespace DoubleStar.Modules.Sales.Application.Commands.VoidSaleCommand;

public sealed record VoidSaleCommand(int SaleId) : IRequest<Result>;