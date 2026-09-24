// .../SendLowStockAlertCommandValidator.cs
namespace DoubleStar.Modules.Notifications.Application.Commands.SendLowStockAlertCommand;

public sealed class SendLowStockAlertCommandValidator : AbstractValidator<SendLowStockAlertCommand>
{
    public SendLowStockAlertCommandValidator()
    {
        RuleFor(x => x.ProductName).NotEmpty();
        RuleFor(x => x.StaffEmail).NotEmpty().EmailAddress();
    }
}