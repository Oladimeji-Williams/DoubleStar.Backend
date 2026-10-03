// Identity/Application/Commands/ConfirmTwoFactorSetupCommand/ConfirmTwoFactorSetupCommand.cs
namespace DoubleStar.Modules.Identity.Application.Commands.ConfirmTwoFactorSetupCommand;
public sealed record ConfirmTwoFactorSetupCommand(string Code) : IRequest<Result>;