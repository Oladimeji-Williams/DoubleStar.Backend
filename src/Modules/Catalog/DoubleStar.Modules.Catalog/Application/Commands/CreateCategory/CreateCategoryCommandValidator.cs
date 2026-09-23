// .../CreateCategoryCommandValidator.cs
namespace DoubleStar.Modules.Catalog.Application.Commands.CreateCategoryCommand;

public sealed class CreateCategoryCommandValidator : AbstractValidator<CreateCategoryCommand>
{
    public CreateCategoryCommandValidator() => RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
}