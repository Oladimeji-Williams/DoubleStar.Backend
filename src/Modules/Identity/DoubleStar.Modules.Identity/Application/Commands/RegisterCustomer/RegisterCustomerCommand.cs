using DoubleStar.SharedKernel.Common.Primitives;
using DoubleStar.SharedKernel.Contracts.Identity;
using MediatR;

namespace DoubleStar.Modules.Identity.Application.Commands.RegisterCustomerCommand;

public sealed record RegisterCustomerCommand(
    string Email,
    string Password,
    string? TurnstileToken)
    : IRequest<Result<RegistrationOutcome>>;