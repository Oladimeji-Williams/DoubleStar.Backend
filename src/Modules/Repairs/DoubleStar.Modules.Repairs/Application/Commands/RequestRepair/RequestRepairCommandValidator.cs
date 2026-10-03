// .../RequestRepairCommandValidator.cs
namespace DoubleStar.Modules.Repairs.Application.Commands.RequestRepairCommand;

public sealed class RequestRepairCommandValidator : AbstractValidator<RequestRepairCommand>
{
    public RequestRepairCommandValidator()
    {
        RuleFor(x => x.DeviceDescription).NotEmpty().MaximumLength(200);
        RuleFor(x => x.ImeiOrSerial).MaximumLength(100);
        RuleFor(x => x.FaultDescription).NotEmpty().MaximumLength(2000);
    }
}