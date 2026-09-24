// .../SendRepairStatusUpdateCommandHandler.cs
using DoubleStar.SharedKernel.Contracts.Customers;
using DoubleStar.SharedKernel.Contracts.Repairs;
using DoubleStar.Modules.Notifications.Application.Services;
using DoubleStar.Modules.Notifications.Application.Templates;

namespace DoubleStar.Modules.Notifications.Application.Commands.SendRepairStatusUpdateCommand;

/// <summary>Manual re-send — e.g. staff clicking "resend SMS" if the customer says they never got it.</summary>
public sealed class SendRepairStatusUpdateCommandHandler(
    IRepairHistory repairHistory, ICustomerDirectory customerDirectory, NotificationDispatcher dispatcher)
    : IRequestHandler<SendRepairStatusUpdateCommand, Result>
{
    public async Task<Result> Handle(SendRepairStatusUpdateCommand request, CancellationToken cancellationToken)
    {
        var ticket = await repairHistory.GetByIdAsync(request.TicketId, cancellationToken);
        if (ticket is null)
        {
            return Result.Failure(new Error(
                "Notifications.TicketNotFound", $"Repair ticket '{request.TicketId}' was not found.", ErrorType.NotFound));
        }
        if (ticket.CustomerId is null)
        {
            return Result.Failure(new Error(
                "Notifications.NoCustomer", "This ticket has no customer to notify.", ErrorType.Validation));
        }

        var customer = await customerDirectory.GetByIdAsync(ticket.CustomerId.Value, cancellationToken);
        if (customer is null)
        {
            return Result.Failure(new Error(
                "Notifications.CustomerNotFound", "Customer record was not found.", ErrorType.NotFound));
        }

        var (subject, html) = EmailTemplates.RepairStatusUpdate(customer.Name, ticket.Id, ticket.Status);

        if (!string.IsNullOrEmpty(customer.Email))
        {
            await dispatcher.SendEmailAsync(customer.Email, subject, html, "RepairStatusUpdate", cancellationToken);
        }
        if (!string.IsNullOrEmpty(customer.Phone))
        {
            await dispatcher.SendSmsAsync(
                customer.Phone, SmsTemplates.RepairStatusUpdate(ticket.Id, ticket.Status), "RepairStatusUpdate", cancellationToken);
        }

        return Result.Success();
    }
}