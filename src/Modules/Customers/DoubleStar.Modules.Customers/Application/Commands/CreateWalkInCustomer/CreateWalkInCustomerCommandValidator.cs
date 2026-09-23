// .../CreateWalkInCustomerCommandValidator.cs
namespace DoubleStar.Modules.Customers.Application.Commands.CreateWalkInCustomerCommand;

public sealed class CreateWalkInCustomerCommandValidator : AbstractValidator<CreateWalkInCustomerCommand>
{
    public CreateWalkInCustomerCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Phone).MaximumLength(20);
        RuleFor(x => x.Email).EmailAddress().When(x => !string.IsNullOrEmpty(x.Email));
    }
}