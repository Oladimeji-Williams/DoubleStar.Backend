// Application/Commands/SendLowStockAlertCommand/SendLowStockAlertCommand.cs
namespace DoubleStar.Modules.Notifications.Application.Commands.SendLowStockAlertCommand;

public sealed record SendLowStockAlertCommand(string ProductName, int AvailableQuantity, string StaffEmail) : IRequest<Result>;