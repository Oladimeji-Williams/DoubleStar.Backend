// .../SendSaleReceiptCommandHandler.cs
using DoubleStar.SharedKernel.Contracts.Customers;
using DoubleStar.Modules.Notifications.Application.Services;
using DoubleStar.Modules.Notifications.Application.Templates;

namespace DoubleStar.Modules.Notifications.Application.Commands.SendSaleReceiptCommand;

public sealed class SendSaleReceiptCommandHandler(ICustomerDirectory customerDirectory, NotificationDispatcher dispatcher)
    : IRequestHandler<SendSaleReceiptCommand, Result>
{
    public async Task<Result> Handle(SendSaleReceiptCommand request, CancellationToken cancellationToken)
    {
        var customer = await customerDirectory.GetByIdAsync(request.CustomerId, cancellationToken);
        if (customer is null)
        {
            return Result.Failure(new Error(
                "Notifications.CustomerNotFound", "Customer record was not found.", ErrorType.NotFound));
        }
        if (string.IsNullOrEmpty(customer.Email))
        {
            return Result.Failure(new Error(
                "Notifications.NoEmail", "This customer has no email on file to send a receipt to.", ErrorType.Validation));
        }

        var (subject, html) = EmailTemplates.SaleReceipt(customer.Name, request.SaleId, request.TotalKobo);
        await dispatcher.SendEmailAsync(customer.Email, subject, html, "SaleReceipt", cancellationToken);

        return Result.Success();
    }
}