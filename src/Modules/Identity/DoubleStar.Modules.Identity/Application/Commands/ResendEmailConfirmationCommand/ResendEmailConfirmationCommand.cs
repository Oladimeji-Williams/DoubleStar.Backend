
using DoubleStar.SharedKernel.Common.Primitives;
using MediatR;

namespace DoubleStar.Modules.Identity.Application.Commands.ResendEmailConfirmationCommand;

public sealed record ResendEmailConfirmationCommand(
    string Email)
    : IRequest<Result>;