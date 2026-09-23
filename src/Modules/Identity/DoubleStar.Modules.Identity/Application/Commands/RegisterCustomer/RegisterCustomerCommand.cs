// Application/Commands/RegisterCustomerCommand/RegisterCustomerCommand.cs
using DoubleStar.Modules.Identity.Application.DTOs;

namespace DoubleStar.Modules.Identity.Application.Commands.RegisterCustomerCommand;

public sealed record RegisterCustomerCommand(
    string FirstName, string LastName, string Email, string? Phone, string Password) : IRequest<Result<Guid>>;