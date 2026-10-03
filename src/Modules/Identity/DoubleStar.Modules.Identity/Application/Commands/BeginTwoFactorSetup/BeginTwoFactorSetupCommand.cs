// Identity/Application/Commands/BeginTwoFactorSetupCommand/BeginTwoFactorSetupCommand.cs
using DoubleStar.SharedKernel.Contracts.Identity;
namespace DoubleStar.Modules.Identity.Application.Commands.BeginTwoFactorSetupCommand;
public sealed record BeginTwoFactorSetupCommand : IRequest<Result<TwoFactorSetupDto>>;