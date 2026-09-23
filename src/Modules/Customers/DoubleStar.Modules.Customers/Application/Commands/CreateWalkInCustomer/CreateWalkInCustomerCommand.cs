// Application/Commands/CreateWalkInCustomerCommand/CreateWalkInCustomerCommand.cs
using DoubleStar.SharedKernel.Contracts.Customers;

namespace DoubleStar.Modules.Customers.Application.Commands.CreateWalkInCustomerCommand;

public sealed record CreateWalkInCustomerCommand(string Name, string? Phone, string? Email)
    : IRequest<Result<CustomerSummaryDto>>;