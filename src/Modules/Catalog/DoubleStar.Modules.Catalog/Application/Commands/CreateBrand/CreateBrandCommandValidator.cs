// .../CreateBrandCommandValidator.cs
namespace DoubleStar.Modules.Catalog.Application.Commands.CreateBrandCommand;

public sealed class CreateBrandCommandValidator : AbstractValidator<CreateBrandCommand>
{
    public CreateBrandCommandValidator() => RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
}