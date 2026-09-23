// .../UpdateCustomerCommandHandler.cs
using DoubleStar.Modules.Customers.Application.Abstractions;
using DoubleStar.Modules.Customers.Application.Errors;

namespace DoubleStar.Modules.Customers.Application.Commands.UpdateCustomerCommand;

public sealed class UpdateCustomerCommandHandler(ICustomerRepository customerRepository)
    : IRequestHandler<UpdateCustomerCommand, Result>
{
    public async Task<Result> Handle(UpdateCustomerCommand request, CancellationToken cancellationToken)
    {
        var customer = await customerRepository.GetByIdAsync(request.Id, cancellationToken);
        if (customer is null)
        {
            return Result.Failure(CustomerErrors.NotFound(request.Id));
        }

        customer.UpdateDetails(request.Name, request.Phone, request.Email, request.Address);
        await customerRepository.UpdateAsync(customer, cancellationToken);

        return Result.Success();
    }
}