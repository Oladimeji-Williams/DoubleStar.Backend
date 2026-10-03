// .../SubmitReviewCommandValidator.cs
namespace DoubleStar.Modules.Reviews.Application.Commands.SubmitReviewCommand;

public sealed class SubmitReviewCommandValidator : AbstractValidator<SubmitReviewCommand>
{
    public SubmitReviewCommandValidator()
    {
        RuleFor(x => x.Rating).InclusiveBetween(1, 5);
        RuleFor(x => x.Comment).NotEmpty().MaximumLength(1000);
    }
}