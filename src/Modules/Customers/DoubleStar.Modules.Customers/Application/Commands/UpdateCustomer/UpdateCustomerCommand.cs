namespace DoubleStar.Modules.Customers.Application.Commands.UpdateCustomerCommand;

public sealed record UpdateCustomerCommand(Guid Id, string Name, string? Phone, string? Email, string? Address)
    : IRequest<Result>;