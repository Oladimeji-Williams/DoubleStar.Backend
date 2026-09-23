// Application/EventHandlers/CustomerAccountRegisteredEventHandler.cs
using DoubleStar.SharedKernel.Contracts.Identity;
using DoubleStar.Modules.Customers.Application.Abstractions;
using DoubleStar.Modules.Customers.Domain.Entities;

namespace DoubleStar.Modules.Customers.Application.EventHandlers;

public sealed class CustomerAccountRegisteredEventHandler(ICustomerRepository customerRepository)
    : INotificationHandler<CustomerAccountRegisteredEvent>
{
    public async Task Handle(CustomerAccountRegisteredEvent notification, CancellationToken cancellationToken)
    {
        if (await customerRepository.GetByIdAsync(notification.UserId, cancellationToken) is not null)
        {
            return; // already linked — safe to receive this more than once
        }

        var customer = Customer.CreateForAccount(
            notification.UserId,
            $"{notification.FirstName} {notification.LastName}".Trim(),
            notification.Phone,
            notification.Email);

        await customerRepository.AddAsync(customer, cancellationToken);
    }
}