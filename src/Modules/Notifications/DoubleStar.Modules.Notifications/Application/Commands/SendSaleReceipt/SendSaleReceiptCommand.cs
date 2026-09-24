// Application/Commands/SendSaleReceiptCommand/SendSaleReceiptCommand.cs
namespace DoubleStar.Modules.Notifications.Application.Commands.SendSaleReceiptCommand;

public sealed record SendSaleReceiptCommand(Guid CustomerId, int SaleId, long TotalKobo) : IRequest<Result>;