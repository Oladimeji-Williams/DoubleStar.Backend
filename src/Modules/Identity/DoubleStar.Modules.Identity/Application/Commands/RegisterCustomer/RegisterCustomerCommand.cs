// RegisterCustomerCommand.cs — final shape
using DoubleStar.Modules.Identity.Infrastructure.Turnstile;
namespace DoubleStar.Modules.Identity.Application.Commands.RegisterCustomerCommand;
public sealed record RegisterCustomerCommand(string Email, string Password, string? TurnstileToken) : IRequest<Result<Guid>>;