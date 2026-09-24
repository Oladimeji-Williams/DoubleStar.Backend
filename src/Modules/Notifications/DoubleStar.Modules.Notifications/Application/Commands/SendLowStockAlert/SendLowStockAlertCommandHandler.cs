// .../SendLowStockAlertCommandHandler.cs
using DoubleStar.Modules.Notifications.Application.Services;
using DoubleStar.Modules.Notifications.Application.Templates;

namespace DoubleStar.Modules.Notifications.Application.Commands.SendLowStockAlertCommand;

public sealed class SendLowStockAlertCommandHandler(NotificationDispatcher dispatcher)
    : IRequestHandler<SendLowStockAlertCommand, Result>
{
    public async Task<Result> Handle(SendLowStockAlertCommand request, CancellationToken cancellationToken)
    {
        var (subject, html) = EmailTemplates.LowStockAlert(request.ProductName, request.AvailableQuantity);
        await dispatcher.SendEmailAsync(request.StaffEmail, subject, html, "LowStockAlert", cancellationToken);
        return Result.Success();
    }
}