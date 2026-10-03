using MediatR;
using DoubleStar.SharedKernel.Common;

namespace DoubleStar.Modules.Identity.Application.Commands.RequestPasswordResetCommand;

public sealed record RequestPasswordResetCommand(
    string Email,
    string? TurnstileToken) : IRequest<Result>;