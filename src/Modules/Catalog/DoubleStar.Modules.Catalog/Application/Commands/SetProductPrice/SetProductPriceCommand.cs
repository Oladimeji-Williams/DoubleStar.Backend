// Application/Commands/SetProductPriceCommand/SetProductPriceCommand.cs
namespace DoubleStar.Modules.Catalog.Application.Commands.SetProductPriceCommand;

public sealed record SetProductPriceCommand(int Id, long UnitPriceKobo) : IRequest<Result>;