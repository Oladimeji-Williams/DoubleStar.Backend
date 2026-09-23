// .../OpenRepairTicketCommandValidator.cs
namespace DoubleStar.Modules.Repairs.Application.Commands.OpenRepairTicketCommand;

public sealed class OpenRepairTicketCommandValidator : AbstractValidator<OpenRepairTicketCommand>
{
    public OpenRepairTicketCommandValidator()
    {
        RuleFor(x => x.DeviceDescription).NotEmpty().MaximumLength(200);
        RuleFor(x => x.ImeiOrSerial).MaximumLength(100);
        RuleFor(x => x.FaultDescription).NotEmpty().MaximumLength(2000);
        RuleFor(x => x.WalkInName).MaximumLength(200);
        RuleFor(x => x.WalkInPhone).MaximumLength(20);
    }
}