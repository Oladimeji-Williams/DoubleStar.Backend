using DoubleStar.SharedKernel.Contracts.Identity;
using DoubleStar.Modules.Customers.Application.Abstractions;
using DoubleStar.Modules.Customers.Domain.Entities;
using MediatR;

namespace DoubleStar.Modules.Customers.Application.EventHandlers;

public sealed class CustomerAccountRegisteredEventHandler(
    ICustomerRepository customerRepository)
    : INotificationHandler<CustomerAccountRegisteredEvent>
{
    public async Task Handle(
        CustomerAccountRegisteredEvent notification,
        CancellationToken cancellationToken)
    {
        if (await customerRepository.GetByIdAsync(
                notification.UserId,
                cancellationToken) is not null)
        {
            return;
        }

        var customer = Customer.CreateForAccount(
            notification.UserId,
            notification.Email);

        await customerRepository.AddAsync(
            customer,
            cancellationToken);
    }
}